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
    *   實作 `PersistentComponentState` 以優化 Prerendering 階段的資料獲取，避免重複呼叫 API。

*   **API Service (`AspireAppTemplate.ApiService`)**:
    *   採用 **FastEndpoints** 構建輕量級、垂直切片的後端 API。
    *   受 **JWT Bearer Token** 保護，驗證由 Keycloak 簽發的 Token。

*   **Keycloak (Identity Provider)**:
    *   以 Docker 容器運行，提供 IAM 服務。
    *   **控制台管理員 (Admin Console)**: `admin` / `admin` (於 `AppHost.cs` 設定)。
    *   **預設應用程式使用者 (App User)**: `admin` / `admin` (預設擁有 `Administrator` 角色，可用於登入系統)。
    *   啟動時會自動匯入 `AspireAppTemplate.AppHost/Realms` 中的 Realm 設定，其中包含自動映射 Role Claim 的配置，確保 .NET `[Authorize(Roles)]` 能直接運作。

*   **Redis (Cache)**:
    *   以 Docker 容器運行，提供分散式快取服務。

## 關鍵技術 (Key Technologies)

*   **.NET 10**
*   **.NET Aspire**: 用於建構可觀察、分散式的雲端原生應用。
*   **Keycloak**: 開源的身分與存取管理解決方案。
*   **Blazor**: 使用 C# 建構互動式 Web UI 的框架。
*   **FastEndpoints**: 用於取代傳統 Controller 的 API 開發框架。
*   **Serilog**: 強大且結構化的日誌記錄系統。
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

啟動後：
1.  瀏覽器會自動開啟 **Aspire Dashboard**。
2.  您可以在 Dashboard 中看到 `webfrontend`, `apiservice`, `keycloak`, `cache` 等服務的狀態與 Endpoint。
3.  點擊 `webfrontend` 的 Endpoint 即可存取應用程式。

## 開發指南 (Development Guidelines)

*   **日誌記錄 (Logging)**:
    *   所有服務均整合了 **Serilog**。
    *   為了避免主控台出現重複日誌，初始化時會調用 `builder.Logging.ClearProviders()`。
    *   透過 `writeToProviders: true` 確保日誌能正確流向 OpenTelemetry (OTLP) 並顯示在 Aspire Dashboard。

*   **Blazor 資料獲取**:
    *   由於預設開啟 **Prerendering**，`OnInitializedAsync` 會執行兩次。
    *   請務必實作 `PersistentComponentState` 機制來快取首屏資料，以提升效能並減少 API 負擔。

*   **身份驗證配置**:
    *   Web 端設定位於 `AspireAppTemplate.Web/Program.cs`。
    *   API 端設定位於 `AspireAppTemplate.ApiService/Program.cs`。

*   **解決方案結構**:
    *   `.slnx` 檔案: 採用新的簡化 XML 格式定義解決方案。
    *   `ServiceDefaults`: 包含 OpenTelemetry、HealthChecks 等標準化配置。

---
*Created by Gemini CLI - 2025/12/25*