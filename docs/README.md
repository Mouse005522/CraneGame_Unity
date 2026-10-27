# Unity Web 本機啟動器（Windows）

把 `StartUnityWeb.cmd`、`StartUnityWeb.ps1` 複製到 Unity Web 輸出的 HTML 同一層，再雙擊 `StartUnityWeb.cmd`。

```text
WebBuild/
├─ StartUnityWeb.cmd
├─ StartUnityWeb.ps1
├─ index.html
├─ Build/
└─ TemplateData/
```

- 不需安裝 Python 或 Node.js，使用 Windows 內建 Windows PowerShell 5.1。
- 自動開啟 `http://127.0.0.1:8080/index.html`，固定使用指定埠，不自動改用其他埠。
- 埠若被舊的 `StartUnityWeb.ps1` PowerShell 服務占用，會停止舊服務後重新啟動；其他程式占用時會顯示錯誤並保留該程式。
- 保持命令視窗開啟。按 Ctrl+C 或關閉視窗即可停止。
- 支援 WASM MIME、`.gz`／`.br` 的 Content-Encoding，以及跨來源隔離標頭。
- `.unityweb` 檔案保留給 Unity loader 自行解壓縮，不加 Content-Encoding。
- 僅服務本機，不對區域網路公開。這是測試用靜態檔案伺服器，並非正式網站服務。

HTML 名稱不是 `index.html`，或要指定埠時，可在該資料夾的終端執行：

```powershell
.\StartUnityWeb.cmd -Page game.html -Port 9000
```

跨網域的 Room API 仍需自行允許 CORS；啟動器不會代理 API。若 HTML 載入外部資源，它們也必須符合 COEP 的跨來源要求。
