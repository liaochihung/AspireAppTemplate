# AspireAppTemplate (AI Context)

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

## 常見任務 (Common Tasks)

### 1. 新增資料表
1.  在 `AspireAppTemplate.Shared` 定義 Model (若需前後端共用) 或在 `AspireAppTemplate.Database` 定義 Entity。
2.  在 `AspireAppTemplate.Database.AppDbContext` 中加入 `DbSet<T>`。
3.  若使用 Migrations，需針對 Database 專案執行 EF Core 指令。

### 2. 運行 API 測試
*   確保 Docker 容器已啟動 (至少 Postgres)。
*   直接執行 `AspireAppTemplate.ApiService` 或透過 `run.bat` 啟動完整 Aspire 環境。
