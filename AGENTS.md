# Codex 專案執行準則

本檔適用於此 repository 與其子目錄。由 `.github/copilot-instructions.md` 轉換，並加入使用者確認的程式碼風格。使用者在對話中的明確要求優先於本檔。

## 專案語境

- 專案：CraneGame。
- 引擎：Unity 6；實際版本以 `ProjectSettings/ProjectVersion.txt` 為準。
- 保留現有 EasyInject、UniTask 與既有生命週期／連線流程，不為局部需求重建架構。

## 禁止編譯

- 不執行 `dotnet build`、`msbuild`、Solution 編譯或 Unity Editor 命令列編譯。
- 不另外建立暫時專案來編譯驗證 Unity 專案程式碼。
- 編譯與執行正確性由使用者在 Unity Editor 手動確認。
- 修改後以程式碼閱讀、呼叫端與型別檢查、diff 等靜態方式檢查，回報時清楚說明未執行編譯。
- 直接修改檔案，不需要為了驗證修改而啟動編譯。

## 搜尋與 Unity 資產

- 優先使用 `rg`／`rg --files` 搜尋程式碼與檔案；必要時使用 PowerShell 檔案查詢。
- 避免修改 `.meta`，除非新增、搬移資產等任務確有需要；搬移資產時保留原 GUID。
- 不修改 Unity 自動產生的專案檔、快取或產生碼來實作功能。

## 語言與溝通

- 對話、解釋、建議與計畫使用繁體中文。
- 新增的程式碼註解使用中文。
- Unity 與 API 專有名詞保留英文，例如 Prefab、GameObject、ScriptableObject、URP、UniTask。

## 程式碼風格

### 展開控制流程與方法

- 使用 Allman 風格：大括號獨立成行，縮排四個空白。
- `if`、`else`、`for`、`foreach`、`while`、`fixed`、`using`、`catch`、`finally` 即使只有一個敘述，也要使用大括號並展開成多行。
- 方法、建構式與 `Dispose` 等轉呼叫方法使用完整方法本體，避免 `=>` 單行方法。
- 不把條件判斷、`return`、`throw`、事件呼叫或數個敘述擠在同一行。
- `switch` 的 case 標籤、執行敘述與 `break`／`return` 分行。
- 方法之間保留空白行。enum 成員、巢狀資料類別分行列出。
- 簡單唯讀屬性可保留 `=>`；一般方法與含控制流程的邏輯不要使用單行寫法。自動屬性可保留原形式。
- callback 若包含轉換、驗證或資源操作，使用多行 lambda 或具名方法，方便下中斷點。

```csharp
public unsafe bool Send (ReadOnlySpan<byte> data)
{
    fixed (byte* pointer = data)
    {
        return Send ((IntPtr)pointer, data.Length);
    }
}

public void Dispose ()
{
    Close ();
}

internal void Receive (byte[] bytes)
{
    if (State != EasyWebRTCChannelState.Closed)
    {
        OnDataReceived?.Invoke (bytes);
    }
}
```

### 基底方法

- 覆寫有基底實作的 async virtual 方法時，在方法開頭呼叫並等待 `base`，即使目前基底邏輯為空也保留呼叫。
- abstract 方法沒有可呼叫的基底實作，不對 abstract 成員產生 `base` 呼叫。

```csharp
protected override async UniTask Setup ()
{
    await base.Setup ();
    // 子類別邏輯。
}
```

### UI 文字

- 會顯示於 UI 的固定文字不得硬編碼。
- 使用 `[SerializeField] LocalizedString`，讓使用者在 Inspector 設定 Localization Table 與 Key。
- 透過 `GetLocalizedString ()` 取得顯示文字；數字等動態值依既有 UI 流程處理。

### 介面實作

- 新增或修改介面實作時使用顯式介面實作，避免把僅供介面使用的成員暴露在類別公開 API。
- 類別內部也需要使用該邏輯時，抽出 private 方法供內部與介面共用。
- 已由使用者明確要求公開的 API 保持公開；格式整理不任意改變既有公開 API。

### 引數與巢狀呼叫

- 優先使用位置引數。
- 只有省略中間可選引數，或順序容易混淆時，才使用具名引數。
- 引數包含方法呼叫、建構式或較長判斷時，先以區域變數拆出結果，再傳給外層 API，便於閱讀與偵錯。

### TryGet 系列 API

- 回傳 bool 的 `TryGetXxx` 使用正向 `if (TryGetXxx (...))` 與 `else`，不寫反向條件後提前返回。
- 失敗分支以 `Debug.LogError (...)` 記錄足夠的 key、index 等資訊。

```csharp
if (table.TryGetValue (key, out var value))
{
    ApplyValue (value);
}
else
{
    Debug.LogError ($"找不到 key = {key} 對應的資料。");
}
```

### RuntimeValue 綁定

- 同一個 class 內，相同泛型型別的 `RuntimeValue<T>` 欄位必須使用不同的 `ValueBinding` index。
- 例如兩個 `RuntimeValue<float>` 不可共用同一個 index，避免序列化或映射混淆。

## 平台與抽象

- 在可編譯的前提下，優先使用 `Application.platform` 判斷平台；只在平台相依型別無法編譯等必要處使用少量 `#if`。
- 優先採取容易閱讀、偵錯的直接寫法；不要為小型平台分支新增不必要的 Factory、註冊或反射機制。
- 平台橋接器只處理平台 API 的映射，業務流程留在原本的管理類別。
