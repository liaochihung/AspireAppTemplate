# AspireAppTemplate (AI Context)

## Keycloak 測試帳號密碼
keycloak console: admin/admin, http://localhost:8080
login user: jack/0000, 

## 架構決策紀錄 (ADR - Architecture Decision Records)

### Project Structure Reorganization (2025-12-27)
*   **`src/` 統一管理**: 所有專案程式碼移至 `src/` 目錄，清楚區分原始碼與配置檔案。
*   **分類目錄結構**:
    *   `src/api/` - 後端 API 服務 (ApiService)
    *   `src/web/` - 前端應用 (BlazorApp，未來可加 Vue/React)
    *   `src/aspire/` - Aspire 編排 (AppHost, ServiceDefaults)
    *   `src/shared/` - 跨專案共用程式碼 (Shared)
    *   `src/tests/` - 測試專案
*   **設計理念**: 參考 MyDotnetStarterKit 的組織結構，但**不採用 Clean Architecture**，保持 KISS 原則。
*   **未來擴展性**:
    *   多前端支援: `src/web/VueApp/`, `src/web/ReactApp/`
    *   模組化 API: 未來可在 `src/api/` 下建立多個模組 (Products, Orders)
    *   微服務準備: 模組可輕鬆拆分成獨立服務
*   **Database 整合**: ~~獨立專案已移除~~，DbContext 已合併到 `src/api/ApiService/Data/`，符合 KISS 原則。


### Database Integration (2025-12-25)
*   **獨立專案**: 資料庫邏輯被拆分至獨立的 Class Library (`AspireAppTemplate.Database`) 以保持關注點分離 (Separation of Concerns)。
*   **固定埠口**: 開發環境下，PostgreSQL 容器強制映射至主機的 `5436` 埠口。
    *   *原因*: 允許開發者使用本地資料庫管理工具 (如 pgAdmin, DBeaver) 直接連線，或在不啟動 AppHost 的情況下獨立運行 `ApiService`。
*   **無連接字串配置**: `ApiService` 的 `appsettings.json` 中**不包含**連接字串。
    *   *機制*: 完全依賴 Aspire 的 Service Discovery 與環境變數注入 (`builder.AddNpgsqlDbContext<AppDbContext>("aspiredb")`)。
*   **預設憑證**:
    *   User: `postgres`
    *   Pass: `1111`
    *   DB: `aspiredb`
*   **Health Check**: 已整合 `AddDbContextCheck<AppDbContext>()` 確保 `/health` 端點驗證資料庫連線。
*   **Endpoints**: 所有 Product Endpoints 直接注入 `AppDbContext` 進行 CRUD 操作。

### Identity Management (2025-12-27)
*   **Keycloak**: 使用 Docker 容器運行。
    *   預設 Admin: `admin` / `admin`
    *   Realm 設定位於 `src/aspire/AppHost/Realms/import-realmdata.json`，由 AppHost 自動匯入。
    *   自訂 Theme 位於 `src/aspire/AppHost/keycloak-themes/my-company-theme/`，需在 Keycloak Admin 手動啟用。
    *   *設計理念*: 所有 Keycloak 相關配置集中在 AppHost 目錄下，保持內聚性與一致性。

### UI Framework (2025-12-26)
*   **MudBlazor 8.2.0**: 取代 Bootstrap CSS，提供完整 Material Design 元件庫。
*   **Element Plus 風格配色**: 主色 `#409EFF`，使用 4px 圓角，支援中文字體。
*   **深色/淺色切換**: 內建於 `MainLayout.razor`，AppBar 右側有切換按鈕。
*   **Theme 設定架構** (對齊 MyDotnetStarterKit):
    *   `Components/ThemeManager/` - 模組化主題元件
        *   `ThemeDrawer.razor` - 主題設定抽屜容器
        *   `DarkModePanel.razor` - 深色模式切換
        *   `ColorPanel.razor` - 主色/輔色選擇器 (可重用)
        *   `RadiusPanel.razor` - 圓角調整
        *   `TableCustomizationPanel.razor` - 表格樣式設定
    *   `Infrastructure/Themes/CustomColors.cs` - 預設顏色清單
    *   `Infrastructure/Settings/UserPreferences.cs` - 使用者偏好設定 (含 TablePreference)
    *   `Infrastructure/Services/LayoutService.cs` - 偏好設定持久化與套用

### API Development Pattern (2025-12-27)
*   **REPR Pattern (Request-Endpoint-Response)**: 採用 FastEndpoints 的 "Folder per Endpoint" 結構。
    *   慣例: `src/api/ApiService/Features/<Feature>/<Action>/Endpoint.cs`
    *   優點: 高內聚性，相關的 Request/Response/Validator/Mapper 放在同一個目錄下。
    *   *範例*: `Features/Identity/Roles/Create/Endpoint.cs`
*   **Authorization 策略**:
    *   **Policy-Based**: 不要在 Endpoint 直接寫死角色 (`AllowAnonymous` 或 `Roles("Admin")`)，而是使用 Policy (`Policies(AppPolicies.CanManageRoles)`)。
    *   **中央配置**: Policy 與角色的對應關係在 `Program.cs` 中定義 (e.g., `options.AddPolicy(...)`)。
    *   **優點**: 解耦權限概念與具體角色，方便未來調整權限邏輯而不需修改每個 Endpoint。

### Identity Management Improvements (2025-12-28)
*   **Authentication Flow Optimization**:
    *   **Service Discovery**: ApiService 優先使用 Aspire 注入的 `services:keycloak:http` 端點連接 Keycloak，解決 JWKS 簽章驗證失敗問題 (`IDX10500`)。
    *   **Claim Mapping 簡化**: 在 `JwtBearerOptions` 中設置 `MapInboundClaims = false`，停用微軟預設的 Claim Type 轉換。
        *   *原因*: 確保 Token 中的 `role` 宣告保持原樣 (e.g., `"role": "Administrator"` -> `type: "role", value: "Administrator"`)，而非被轉換為 `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`。這簡化了前後端對 Claims 的認知一致性。
    *   **Refresh Token**: 前端 `AuthorizationHandler` 實作了 Access Token 自動刷新機制 (小於 1 分鐘過期時自動刷新)。
*   **Token Audience Security**: 
    *   **Realm Config**: 在 `import-realmdata.json` 為 `WeatherWeb` 客戶端添加了 `oidc-hardcoded-audience-mapper`，強制將 `weather.api` 加入 Token 的 Audience (`aud`)。
    *   *未來啟用*: 待 Keycloak 容器重建生效後，即可在 API 端啟用 `VerifyTokenAudience: true` 以增強安全性。

### API Documentation (2025-12-27)
*   **Scalar**: 取代 Swagger UI 作為主要的 API 文件介面。
    *   *原因*: 提供更好的視覺體驗 (Rich Aesthetics) 與 DX (Developer Experience),符合專案的 Premium 定位。
    *   *整合方式*: 使用 `Scalar.AspNetCore` 並對接 FastEndpoints 生成的 OpenAPI JSON (`/swagger/v1/swagger.json`)。
    *   *主題*: 預設使用 `Moon` (深色) 主題。

### Background Jobs Management (2025-12-29)
*   **Hangfire 1.8.20**: 採用 Hangfire 作為背景任務管理系統。
    *   *選型理由*: 支援動態任務建立、持久化儲存、內建 Dashboard 與失敗重試機制，適合需要使用者可視化管理的場景。
    *   *儲存體*: 使用 PostgreSQL (`Hangfire.PostgreSql 1.20.13`)，與業務資料庫共用連接字串，但使用獨立的 `hangfire` schema。
*   **Dashboard 授權**:
    *   路由: `/hangfire`
    *   權限: 僅限 `Administrator` 角色存取 (透過 `HangfireAuthorizationFilter` 實作)
*   **任務範例**:
    *   `LogCleanupJob`: 示範週期性清理任務，支援自動重試 (3 次，間隔 60s/300s/900s)
    *   API Endpoint: `POST /api/jobs/log-cleanup` 允許管理員動態建立清理任務
*   **設計原則**:
    *   Job 類別放置於 `Infrastructure/Jobs/`
    *   API Endpoint 遵循 REPR Pattern: `Features/Jobs/<JobName>/<Action>/`
    *   使用 `AppPolicies.CanManageSystem` 策略保護任務管理 API

### File Storage (2025-12-30)
*   **MinIO**: 作為 S3-compatible 的物件儲存服務。
    *   *架構*: 在 AppHost 中新增 `minio` 容器，並掛載 `minio-data` Volume 確保資料持久化。
    *   *端口*: 固定開放 `9000` (API) 與 `9001` (Console) 以便開發調試。
*   **資料流模式 (Proxy Mode)**:
    *   **Upload**: 前端 -> API Server (Upload Endpoint) -> MinIO (PutObject)。
    *   **Reference**: API 回傳自身 Proxy URL (e.g. `/api/storage/files/...`)。
    *   **Access**: 瀏覽器 -> API Server (Get Endpoint) -> MinIO (GetStream)。
    *   *原因*: 解決 Mixed Content 問題 (HTTPS 網頁無法載入 HTTP MinIO 圖片)，並隱藏 MinIO 真實位置。
*   **介面設計**:
    *   `IStorageService`: 定義標準上傳 `UploadAsync` 與刪除 `Remove` 介面。
    *   `MinioStorageService`: 實作 MinIO 連線，並自動處理 Bucket 建立與 Public Policy 設定。

### Exception Handling & Logging Standardization (2025-12-30)
*   **ProblemDetails**: 全面採用 RFC 7807 標準格式回應錯誤。
    *   Validation Errors: 自動回傳 400 詳細欄位錯誤。
    *   Domain Errors: 透過 `ErrorOrExtensions.SendResultAsync` 將 `ErrorOr` 錯誤映射為 ProblemDetails。
*   **Serilog Request Logging**: 啟用 `app.UseSerilogRequestLogging()`，提供高效能且結構化的 HTTP 請求日誌。
*   **Result Pattern**: 強制使用 `ErrorOr<T>` 進行服務層流控，避免使用 Exception 控制邏輯。

## 常見任務 (Common Tasks)

### 1. 新增資料表
1.  在 `AspireAppTemplate.Shared` 定義 Model (若需前後端共用) 或在 `AspireAppTemplate.Database` 定義 Entity。
2.  在 `AspireAppTemplate.Database.AppDbContext` 中加入 `DbSet<T>`。
3.  若使用 Migrations，需針對 Database 專案執行 EF Core 指令。

### 2. 運行 API 測試
*   確保 Docker 容器已啟動 (至少 Postgres)。
*   直接執行 `AspireAppTemplate.ApiService` 或透過 `run.bat` 啟動完整 Aspire 環境。


# AI 開發指引：BDD 測試優先與規格驅動開發 (SDD)

### 1. 核心開發原則
*   **測試優先 (Test-First)：** 在撰寫功能程式碼前，必須先確保有對應的測試案例。
*   **小步快跑 (Small Steps)：** 保持紅燈 (Red) -> 綠燈 (Green) -> 重構 (Refactor) 的節奏。
*   **規格即模具：** 程式碼行為必須嚴格限制在規格描述的範圍內，避免過度設計。

### 2. 測試策略：以行為為核心 (Behavior-Centric Testing)
*   **測功能，而非函式 (Test Features, Not Functions)**：
    *   測試的顆粒度應對齊「業務行為」（例如：「使用者註冊」），而非內部的實作細節（例如：「Repository 的 Save 方法」或「Service 的驗證函式」）。
    *   **禁止**為了追求覆蓋率而單獨測試 `Service`、`Repository` 等內部類別，除非該邏輯極度複雜且與 I/O 無關。
*   **垂直切片測試 (Vertical Slice Testing)**：
    *   主要測試對象應為 **Endpoint**。
    *   驗證從 Request 輸入 -> Validator -> 業務邏輯 -> Database 持久化 -> Response 輸出的完整路徑。
    *   **Mock 的原則**：只 Mock **外部**不受控的依賴（如：第三方金流 API、寄信服務），對於 Database 與內部邏輯，應盡量使用 Testcontainers 或真實環境進行驗證，確保組件協作正常。

### 3. BDD 開發工作流
針對**複雜功能**，請依循以下思維模式：

1.  **規格拆解 (Given/When/Then)**：
    *   專注於 Feature 的外在行為，將實作細節與業務行為分開。
2.  **失敗測試 (Red Light)**：
    *   撰寫一個針對該 Feature (Endpoint) 的測試，並確認它失敗。
3.  **最小實作 (Green Light)**：
    *   實作 Endpoint 與必要的邏輯，以最精簡的方式通過測試。
4.  **持續重構 (Refactor)**：
    *   檢查壞味道 (Code Smells)。
    *   因為測試是針對「外部行為」，所以你可以放心地重構內部結構（例如抽取 Service 或 Value Object），而不用擔心修改測試程式碼。

### 4. 架構與品質規範 (AspireAppTemplate 專用)
*   **遵循 REPR Pattern (FastEndpoints)**：
    *   **Endpoint 優先**：每個 Endpoint 應為獨立、高內聚的單元，包含相關的 Request/Response/Validator。
    *   **KISS 原則**：不強制拆分 Controller/Service 層，除非邏輯確實需要共用。
*   **領域術語一致性**：使用與業務場景一致的命名，確保 Request/Response DTO 命名清晰。
*   **互動與確認**：若規格包含無法測試的形容詞（如「方便」），請先要求定義驗收標準；若發現程式碼壞味道，請主動建議重構。
*   **例外處理與日誌 (Exception & Logging)**：
    *   **Result Pattern**: 服務層**禁止拋出例外**來控制流程，必須回傳 `ErrorOr<T>`。
    *   **ProblemDetails**: API 錯誤回應必須遵循 RFC 7807 (由 `SendResultAsync` 自動處理)。
    *   **Serilog**: 使用結構化日誌記錄關鍵業務事件，HTTP 請求日誌已自動啟用。