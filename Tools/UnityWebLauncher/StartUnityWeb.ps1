param (
    [ValidateRange(1024, 65535)]
    [int]$Port = 8080,
    [string]$Page = 'index.html',
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
$serverRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$rootPrefix = $serverRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
$entryFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($serverRoot, $Page))

if (-not $entryFile.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
{
    throw 'The HTML page must be inside the launcher folder.'
}
if (-not [System.IO.File]::Exists($entryFile))
{
    throw "HTML file not found: $entryFile. Put both launcher files next to index.html, or pass -Page your-file.html."
}

function Get-FileHeaders ([string]$FilePath)
{
    $contentEncoding = $null
    $contentPath = $FilePath
    if ($contentPath.EndsWith('.gz', [System.StringComparison]::OrdinalIgnoreCase))
    {
        $contentEncoding = 'gzip'
        $contentPath = $contentPath.Substring(0, $contentPath.Length - 3)
    }
    elseif ($contentPath.EndsWith('.br', [System.StringComparison]::OrdinalIgnoreCase))
    {
        $contentEncoding = 'br'
        $contentPath = $contentPath.Substring(0, $contentPath.Length - 3)
    }

    $extension = [System.IO.Path]::GetExtension($contentPath).ToLowerInvariant()
    $contentType = switch ($extension)
    {
        '.html' { 'text/html; charset=utf-8' }
        '.htm' { 'text/html; charset=utf-8' }
        '.js' { 'application/javascript' }
        '.mjs' { 'application/javascript' }
        '.wasm' { 'application/wasm' }
        '.css' { 'text/css; charset=utf-8' }
        '.json' { 'application/json' }
        '.png' { 'image/png' }
        '.jpg' { 'image/jpeg' }
        '.jpeg' { 'image/jpeg' }
        '.svg' { 'image/svg+xml' }
        '.ico' { 'image/x-icon' }
        '.mp4' { 'video/mp4' }
        '.webm' { 'video/webm' }
        '.ogg' { 'audio/ogg' }
        '.mp3' { 'audio/mpeg' }
        default { 'application/octet-stream' }
    }

    return @{ Type = $contentType; Encoding = $contentEncoding }
}

function Write-ResponseHeaders ($Stream, [int]$Code, [string]$Reason, [long]$Length, [string]$Type, [string]$Encoding)
{
    $headers = "HTTP/1.1 $Code $Reason`r`nContent-Type: $Type`r`nContent-Length: $Length`r`n"
    $headers += "Connection: close`r`nCache-Control: no-store`r`n"
    # 讓需要 SharedArrayBuffer 的 Unity Web 輸出具備跨來源隔離條件。
    $headers += "Cross-Origin-Opener-Policy: same-origin`r`nCross-Origin-Embedder-Policy: require-corp`r`n"
    if ($Encoding)
    {
        $headers += "Content-Encoding: $Encoding`r`n"
    }
    $headers += "`r`n"
    $headerBytes = [System.Text.Encoding]::ASCII.GetBytes($headers)
    $Stream.Write($headerBytes, 0, $headerBytes.Length)
}

# 固定使用指定埠；只回收由本啟動腳本開啟的 PowerShell 服務。
$connections = @(Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)
$ownerIds = @($connections | Select-Object -ExpandProperty OwningProcess -Unique)
foreach ($ownerId in $ownerIds)
{
    $owner = Get-CimInstance Win32_Process -Filter "ProcessId = $ownerId"
    $isPowerShell = $owner.Name -in @('powershell.exe', 'pwsh.exe')
    $isLauncher = $owner.CommandLine -match '(?i)-File\s+(?:"[^"\r\n]*[\\/]StartUnityWeb\.ps1"|[^\s"]*[\\/]StartUnityWeb\.ps1)(?:\s|$)'
    if ($ownerId -eq $PID -or -not $isPowerShell -or -not $isLauncher)
    {
        throw "Port $Port is occupied by another program (PID $ownerId). Close that program before retrying."
    }
}
foreach ($ownerId in $ownerIds)
{
    Write-Host "Stopping previous Unity Web server (PID $ownerId) on port $Port."
    Stop-Process -Id $ownerId -ErrorAction Stop
    Wait-Process -Id $ownerId -Timeout 5 -ErrorAction SilentlyContinue
}

# 僅綁定本機，不需要管理員 URL 保留設定，也不對區域網路公開。
$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
$listener.Server.ExclusiveAddressUse = $true
try
{
    $listener.Start()
}
catch
{
    $listener.Stop()
    throw "Cannot bind port $Port. No alternative port will be selected. $($_.Exception.Message)"
}

$pageUrl = ($Page.Replace('\', '/') -split '/' | ForEach-Object { [System.Uri]::EscapeDataString($_ ) }) -join '/'
$url = "http://127.0.0.1:$Port/$pageUrl"

try
{
    Write-Host "Unity Web: $url"
    Write-Host "Folder: $serverRoot"
    Write-Host 'Keep this window open. Press Ctrl+C to stop.'
    if (-not $NoBrowser)
    {
        Start-Process $url
    }

    while ($true)
    {
        if (-not $listener.Pending())
        {
            Start-Sleep -Milliseconds 100
            continue
        }

        $client = $listener.AcceptTcpClient()
        $client.ReceiveTimeout = 5000
        $client.SendTimeout = 15000
        $stream = $client.GetStream()
        try
        {
            # 只接收 GET/HEAD 的 HTTP 標頭，限制長度以免無限等待。
            $request = [System.Text.StringBuilder]::new()
            while ($request.Length -lt 16384)
            {
                $value = $stream.ReadByte()
                if ($value -lt 0)
                {
                    break
                }
                [void]$request.Append([char]$value)
                if ($value -eq 10 -and $request.ToString().EndsWith("`r`n`r`n"))
                {
                    break
                }
            }

            $requestLine = ($request.ToString() -split "`r`n", 2)[0]
            $parts = $requestLine -split ' '
            if ($parts.Length -lt 3 -or ($parts[0] -ne 'GET' -and $parts[0] -ne 'HEAD'))
            {
                Write-ResponseHeaders $stream 405 'Method Not Allowed' 0 'text/plain' ''
                continue
            }

            $requestPath = ($parts[1] -split '\?', 2)[0]
            $relativePath = [System.Uri]::UnescapeDataString($requestPath).TrimStart('/')
            if ([string]::IsNullOrEmpty($relativePath))
            {
                $relativePath = $Page
            }
            $filePath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($serverRoot, $relativePath))
            if (-not $filePath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
            {
                Write-ResponseHeaders $stream 403 'Forbidden' 0 'text/plain' ''
                continue
            }
            if (-not [System.IO.File]::Exists($filePath))
            {
                Write-ResponseHeaders $stream 404 'Not Found' 0 'text/plain' ''
                Write-Host "404 $relativePath"
                continue
            }

            $fileHeaders = Get-FileHeaders $filePath
            $file = [System.IO.File]::OpenRead($filePath)
            try
            {
                Write-ResponseHeaders $stream 200 'OK' $file.Length $fileHeaders.Type $fileHeaders.Encoding
                if ($parts[0] -ne 'HEAD')
                {
                    $file.CopyTo($stream)
                }
            }
            finally
            {
                $file.Dispose()
            }
        }
        catch
        {
            Write-Warning $_.Exception.Message
        }
        finally
        {
            $stream.Dispose()
            $client.Dispose()
        }
    }
}
finally
{
    $listener.Stop()
}
