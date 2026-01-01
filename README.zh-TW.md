# AspireAppTemplate

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[English Version](README.md)

## 項目概述 (Project Overview)

**AspireAppTemplate** 是一個基於 **.NET Aspire** 的微服務應用程式範本，專為現代化雲端原生開發而設計。本專案秉持 **KISS (Keep It Simple, Stupid)** 原則，並整合了完整的全端技術堆疊：前端 (Blazor)、後端 API、身分驗證 (Keycloak)、物件儲存 (MinIO)、分散式快取 (Redis) 以及關聯式資料庫 (PostgreSQL)。

此範本旨在為開發者提供一個穩固、可擴展且易於維護的開發起點。

## 核心功能 (Key Features)

*   **雲端原生編排**: 透過 **.NET Aspire** 實現無縫的本地開發與容器編排。
*   **現代化前端**:
    *   **Blazor Interactive Server**: 高效能的互動式 UI 開發。
    *   **MudBlazor**: 採用類 Element Plus 風格的 Material Design 元件庫。
    *   **Theme Manager**: 內建深色/淺色模式切換與主題色自訂功能。
*   **垂直切片架構 (Vertical Slice)**:
    *   **FastEndpoints**: 採用 **REPR Pattern** (Request-Endpoint-Response) 構建高內聚的後端 API。
    *   **拒絕過度設計**: 移除不必要的抽象層 (如 Clean Architecture 的繁瑣分層)，專注於業務價值。
*   **強大的身分管理**:
    *   **Keycloak**: 完整整合 Docker 化運行的 OpenID Connect (OIDC) 認證服務。
    *   **自訂 Realm 與主題**: 預先設定並在啟動時自動匯入 Realm 與專屬主題 (`aspire-app-theme`)。
*   **資料與儲存**:
    *   **PostgreSQL**: 主要關聯式資料庫，搭配 EF Core 使用。
    *   **MinIO**: S3 相容的物件儲存服務，用於檔案管理。
    *   **Redis**: 用於 OutputCache 與資料快取。
*   **背景任務處理**:
    *   **Hangfire**: 整合持續性的背景任務處理，並具備可視化 Dashboard (使用 Postgres 儲存)。
*   **可觀測性**:
    *   **OpenTelemetry**: 內建追蹤 (Tracing)、指標 (Metrics) 與日誌 (Logging)。
    *   **Serilog**: 全服務結構化日誌。
    *   **Scalar**: 下一代 API 文件與測試介面。

## 專案結構 (Project Structure)

所有程式碼統一管理於 `src/` 目錄下：

*   **`src/api/` (Backend)**
    *   `ApiService`: 核心 API 服務，包含業務邏輯、資料存取 (EF Core) 與 Hangfire 任務。
*   **`src/web/` (Frontend)**
    *   `Web`: Blazor Server 前端應用程式。
*   **`src/aspire/` (Orchestration)**
    *   `AppHost`: Aspire 編排器，負責定義容器與服務依賴。
    *   `ServiceDefaults`: 共用的 Health Checks 與 OpenTelemetry 設定。
*   **`src/shared/`**
    *   跨前後端共用的 DTO 與 Model (最小化耦合)。

## 快速開始 (Getting Started)

### 前置需求 (Prerequisites)

1.  **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (Preview 或最新版)。
2.  **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (必須處於執行狀態)。

### 執行專案 (Running the Application)

專案根目錄提供了一個便捷腳本，可一鍵啟動開發環境：

```cmd
run.bat
```

或使用 CLI 指令：

```bash
dotnet run --project src/aspire/AppHost/AspireAppTemplate.AppHost.csproj
```

### 啟動後流程

1.  **Aspire Dashboard**: 瀏覽器將自動開啟，顯示所有服務 (`webfrontend`, `apiservice`, `postgres`, `keycloak`, `minio`, `cache`) 的健康狀態。
2.  **資料庫建立**: `ApiService` 啟動時會自動建立 `aspiredb` 資料庫並套用 Migrations。
3.  **Keycloak 就緒**: Keycloak 啟動後會自動匯入設定，準備好接受登入請求。

## 服務存取與預設憑證 (Credentials)

本地開發環境的預設帳號密碼如下：

| 服務 (Service) | 網址 (Url) | 帳號 (Username) | 密碼 (Password) | 備註 (Notes) |
| :--- | :--- | :--- | :--- | :--- |
| **Aspire Dashboard** | 自動開啟 | - | - | 查看 Log 與 Trace 的中控台。 |
| **Web Frontend** | `https://localhost:<port>` | `jack` | `0000` | 測試使用者帳號。 |
| **Keycloak Console** | `http://localhost:8080` | `admin` | `admin` | IAM 管理介面。 |
| **MinIO Console** | `http://localhost:9001` | `minioadmin` | `minioadmin` | S3 物件儲存管理介面。 |
| **PostgreSQL** | `localhost:5436` | `postgres` | `1111` | 資料庫名稱: `aspiredb`。 |
| **Hangfire Dashboard** | `/hangfire` (API) | `admin` | - | 需具備 `Administrator` 角色才可存取。 |
| **Scalar (API Docs)** | `/scalar/v1` (API) | - | - | 互動式 API 文件。 |

## 致謝與靈感來源 (Credits & Inspiration)

本專案深受 [FullStackHero Blazor Starter Kit](https://github.com/fullstackhero/blazor-starter-kit) 的啟發。

**為什麼要造這個輪子？**
雖然 Blazor Starter Kit 是一個非常優秀的專案，但其嚴格的 Clean Architecture 架構對於中小型專案或非專職前端的開發者來說，學習曲線較為陡峭。因此，我決定基於 **.NET Aspire** 打造一個更輕量化的版本。本專案採用 **Vertical Slice** 架構，旨在保留現代化雲端原生能力的同時，提供一個更直觀、簡單的開發起點。

## 開發指南 (Development Guide)

### 資料庫 (Database)
*   **EF Core**: DbContext 位於 `src/api/ApiService/Data` 目錄下。
*   **固定埠口**: 本地開發埠口固定為 `5436`，方便使用 pgAdmin 或 Datagrip 連線。

### 檔案儲存 (MinIO)
*   本專案採用 **代理模式 (Proxy Mode)** 處理檔案上傳/下載。
    *   **Frontend** 上傳至 `ApiService`。
    *   **ApiService** 串流轉傳至 **MinIO** (port 9000)。
    *   此設計可避免 Mixed Content 問題 (HTTPS 網頁呼叫 HTTP MinIO)，並封裝儲存邏輯。

### 常見問題 (Troubleshooting)

**Q: 埠口 7092/7085 被佔用，導致 Keycloak 登入失敗。**
*   若 Aspire 分配了隨機埠口 (例如 `7123`)，Keycloak 會拒絕該 Redirect URI。
*   **解決方法**:
    1.  開啟 `src/aspire/AppHost/Realms/import-realmdata.json`。
    2.  將 `https://localhost:<新埠口>` 與 `https://localhost:<新埠口>/signin-oidc/signout-callback-oidc` 加入 `redirectUris`。
    3.  重啟 Keycloak 容器 (若需要，請刪除 Volume 以強制重新匯入)。

**Q: 如何部署到生產環境？**
*   使用環境變數來覆蓋 Keycloak 與 API 的 URL 設定。
*   詳情請參考 `src/aspire/AppHost/AppHost.cs` 中關於 `KC_HOSTNAME_URL` 的註解說明。