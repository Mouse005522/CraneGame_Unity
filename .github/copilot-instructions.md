# Copilot Agent 執行準則 (Unity Project)

## 核心限制：禁止編譯 (No Build Policy)
由於 Unity 專案的特殊性與編譯環境依賴性，請遵守以下行為準則：

1. **嚴禁嘗試 Build 專案**：
   - 絕對不要執行 `dotnet build`, `msbuild` 或任何嘗試編譯整個 Solution 的指令。
   - 絕對不要執行 Unity Editor 的命令列編譯指令。
   
2. **編譯驗證權限**：
   - 所有代碼的正確性與編譯檢查將由「使用者 (User)」在 Unity Editor 端手動確認。
   - 你只需要確保代碼邏輯在 C# 語法上是正確的即可。

## 執行建議 (Instruction Guidelines)
- **程式碼修改**：當需要修改 `.cs` 檔案或 `IFlowable` 等介面實作時，請直接產出程式碼或執行檔案修改，不需要在終端機嘗試驗證。
- **檔案搜尋**：允許使用 `Get-ChildItem` 或 `Select-String` 在 `Assets` 目錄下搜尋結構，但請避免觸動 `.meta` 檔案除非必要。

## 專案語境 (Context)
- 專案名稱：Echo_Canyon
- 引擎版本：Unity 6 (或其他你使用的版本)
- 主要框架：EasyInject / UniTask (若有用到請保留)

## 語言與溝通規範 (Language & Communication)

1. **優先語言 (Primary Language)**: 
   - 所有的解釋、建議、對話與計畫說明，請一律使用 **「繁體中文 (Traditional Chinese, zh-TW)」**。
   - 除非是特定的程式碼專有名詞、編譯錯誤訊息或 API 名稱，否則請勿隨意切換為英文。

2. **程式碼註解 (Code Comments)**: 
   - 產出的程式碼註解請保持使用 **中文** (以符合專案開發習慣)，但對話解釋需為繁體中文。

3. **專業術語 (Terminology)**: 
   - 針對 Unity 相關術語（如：Prefab, Game Object, ScriptableObject, URP）請保留原始英文，不要翻譯成中文，以免造成歧義。

## 程式碼慣例 (Coding Conventions)

1. **Override 虛擬方法時必須呼叫 base**：
   - 當 `override` 一個 `virtual` / `abstract` 的 `async` 方法時，必須在方法開頭加上 `await base.XXX()`。
   - 即使目前基底方法沒有實作邏輯，也要保留呼叫，以確保未來基底類別新增邏輯時不會被跳過。
   - 範例：
     ```csharp
     protected override async UniTask Setup ()
     {
         await base.Setup ();
         // 子類別邏輯...
     }
     ```

2. **UI 顯示文字禁止硬編碼**：
   - 任何會顯示在 UI 上的文字（例如按鈕文字、提示訊息、狀態描述等），一律不得使用硬編碼字串。
   - 應宣告為 `LocalizedString` 並透過 `[SerializeField]` 序列化，讓使用者可在 Inspector 中設定對應的 Localization Table 與 Key。
   - 取得文字時使用 `localizedString.GetLocalizedString()` 方法。
   - 範例：
     ```csharp
     [SerializeField]
     LocalizedString freeTextLocalizedString;

     void UpdateText (int count)
     {
         text.text = count == 0 ? freeTextLocalizedString.GetLocalizedString () : count.ToString ();
     }
     ```

3. **介面使用顯式實作 (Explicit Interface Implementation)**：
   - 當類別實作介面時，一律使用顯式介面實作，不要使用隱式實作。
   - 這可以確保介面成員只能透過介面型別存取，避免意外暴露在類別的公開 API 上。
   - 當類別內部需要呼叫自身的介面成員時，應抽出 `private` 方法供介面實作與內部共用。
   - 範例：
     ```csharp
     public class StepOptionBinding : ISettingOptionBinding
     {
         int ISettingOptionBinding.CurrentIndex => GetCurrentIndex ();

         int GetCurrentIndex ()
         {
             // 內部邏輯...
         }
     }
     ```

4. **呼叫方法或建構式時，優先使用位置引數**：
   - 當方法簽名允許按順序傳入參數時，不要使用具名引數（named argument），直接按順序放入即可。
   - 僅在省略中間可選參數、或順序容易混淆導致可讀性下降時，才使用具名引數。
   - 範例：
     ```csharp
     // ✅ 正確
     new ServiceCollection (0.1f, parentInject);

     // ❌ 避免
     new ServiceCollection (physicsRate: 0.1f, defaultInject: parentInject);
     ```

5. **TryGet 系列 API 必須使用 `if / else` 結構，並在失敗分支印出錯誤**：
   - 凡使用回傳 `bool` 的 `TryGetXxx` 方法時，一律以正向條件 `if (TryGetXxx(...))` 撰寫，禁止使用 `if (!TryGetXxx(...)) return` 的提前返回寫法。
   - `else` 分支必須呼叫 `Debug.LogError(...)` 印出足夠的除錯資訊（例如傳入的 key 或 index）。
   - 範例：
     ```csharp
     // ✅ 正確
     if (modelSetting.Value.TryGetCharacterData (skinIndex, out CharacterData data))
     {
         // 使用 data...
     }
     else
     {
         Debug.LogError ($"找不到 skinIndex {skinIndex} 對應的 CharacterData");
     }

     // ❌ 避免
     if (!modelSetting.Value.TryGetCharacterData (skinIndex, out CharacterData data)) return;
     // 使用 data...

6. **方法內巢狀 API 呼叫應拆成多行**：
   - 當方法呼叫的引數本身又包含方法呼叫、建構式或較長條件式時，不要直接內嵌在同一行完成。
   - 應先以區域變數拆出中間結果，再將該結果傳入外層 API，以提升可讀性與除錯性。
   - 範例：
     ```csharp
     // ✅ 正確
     bool isKeepCurrentLight = IsOddNumberOrder (i) == keepOddLight;
     runtimeRoadLights[i].SetChildActive (isKeepCurrentLight);

     // ❌ 避免
     runtimeRoadLights[i].SetChildActive (IsOddNumberOrder (i) == keepOddLight);

7. **`RuntimeValue<T>` 在同一個 class 內，相同泛型型別的 `ValueBinding` index 必須錯開**：
   - 當同一個資料 class 內有多個相同泛型型別的 `RuntimeValue<T>` 欄位時，不能共用同一個 `ValueBinding` index。
   - 必須針對每一個相同泛型型別的欄位配置不同 index，避免序列化或資料映射混淆。
   - 例如同一個 class 內若有兩個 `RuntimeValue<float>`，則應分別使用不同的 `ValueBinding` index。
     ```
     ```