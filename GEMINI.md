# AspireAppTemplate (AI Context)

## 架構決策紀錄 (ADR - Architecture Decision Records)

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

### Identity Management
*   **Keycloak**: 使用 Docker 容器運行。
    *   預設 Admin: `admin` / `admin`
    *   Realm 設定由 `Realms/import-realmdata.json` 自動匯入。

## 常見任務 (Common Tasks)

### 1. 新增資料表
1.  在 `AspireAppTemplate.Shared` 定義 Model (若需前後端共用) 或在 `AspireAppTemplate.Database` 定義 Entity。
2.  在 `AspireAppTemplate.Database.AppDbContext` 中加入 `DbSet<T>`。
3.  若使用 Migrations，需針對 Database 專案執行 EF Core 指令。

### 2. 運行 API 測試
*   確保 Docker 容器已啟動 (至少 Postgres)。
*   直接執行 `AspireAppTemplate.ApiService` 或透過 `run.bat` 啟動完整 Aspire 環境。
