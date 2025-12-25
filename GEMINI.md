# AspireAppTemplate

## 項目概述 (Project Overview)

本專案是一個基於 **.NET Aspire** 的微服務應用程式範本，展示了現代化的雲端原生開發架構。它整合了前端 (Blazor)、後端 API、身份驗證 (Keycloak) 以及分散式快取 (Redis)，旨在提供一個開箱即用的開發起點。

## 架構與服務 (Architecture & Services)

系統由 `AspireAppTemplate.AppHost` 負責編排 (Orchestration)，包含以下核心組件：

*   **AppHost (`AspireAppTemplate.AppHost`)**:
    *   負責啟動、連接與管理所有微服務容器。
    *   定義服務間的依賴關係 (例如 Web 依賴於 API 和 Cache)。

*   **Web Frontend (`AspireAppTemplate.Web`)**:
    *   基於 **Blazor Interactive Server** 的前端應用程式。
    *   整合 **Keycloak (OpenID Connect)** 進行使用者身分驗證。
    *   使用 `OutputCache` (Redis) 進行頁面級別的快取。
    *   透過 `AuthorizationHandler` 自動管理 HTTP 請求的 Token 傳遞。

*   **API Service (`AspireAppTemplate.ApiService`)**:
    *   提供業務邏輯與數據的後端 API (如 Weather, Products)。
    *   受 **JWT Bearer Token** 保護，驗證由 Keycloak 簽發的 Token。

*   **Keycloak (Identity Provider)**:
    *   以 Docker 容器運行，提供 IAM 服務。
    *   **預設管理員帳號**: `admin` / `admin` (於 `AppHost.cs` 中設定)。
    *   啟動時會自動匯入 `AspireAppTemplate.AppHost/Realms` 中的 Realm 設定。

*   **Redis (Cache)**:
    *   以 Docker 容器運行，提供分散式快取服務。

## 關鍵技術 (Key Technologies)

*   **.NET 10**
*   **.NET Aspire**: 用於建構可觀察、分散式的雲端原生應用。
*   **Keycloak**: 開源的身分與存取管理解決方案。
*   **Blazor**: 使用 C# 建構互動式 Web UI 的框架。
*   **Docker**: 容器化運行環境。

## 快速開始 (Getting Started)

### 前置需求 (Prerequisites)
1.  安裝 **.NET 10 SDK** (或符合專案配置的版本)。
2.  安裝 **Docker Desktop** (或其他相容的容器 Runtime)，並確保 Docker 正在運行。

### 執行專案 (Running the Application)

專案根目錄提供了一個便捷腳本，可一鍵啟動開發環境：

```cmd
run.bat
```

該腳本實際上執行了以下標準 Aspire 啟動指令：

```bash
cd AspireAppTemplate.AppHost
dotnet watch
```

啟動後：
1.  瀏覽器會自動開啟 **Aspire Dashboard**。
2.  您可以在 Dashboard 中看到 `webfrontend`, `apiservice`, `keycloak`, `cache` 等服務的狀態與 Endpoint。
3.  點擊 `webfrontend` 的 Endpoint 即可存取應用程式。

## 開發指南 (Development Guidelines)

*   **解決方案結構**:
    *   `.slnx` 檔案: 採用新的簡化 XML 格式定義解決方案。
    *   `ServiceDefaults`: 包含 OpenTelemetry、HealthChecks 等標準化配置，確保所有服務具備一致的可觀察性。
    
*   **身份驗證配置**:
    *   Web 端設定位於 `AspireAppTemplate.Web/Program.cs`。
    *   API 端設定位於 `AspireAppTemplate.ApiService/Program.cs`。
    *   若需修改 Keycloak Realm (例如 Client ID 或 Roles)，請編輯 `AspireAppTemplate.AppHost/Realms/import-realmdata.json`。

*   **新增服務**:
    *   在 `AspireAppTemplate.AppHost/AppHost.cs` 中使用 `builder.AddProject<T>` (加入 .NET 專案) 或 `builder.AddContainer` (加入 Docker 映像檔)。

---
*Created by Gemini CLI - 2025/12/25*
