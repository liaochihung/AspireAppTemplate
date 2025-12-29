using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace AspireAppTemplate.Web.Components.Layout
{
    public partial class NavMenu
    {
        [Parameter]
        public bool IsOpen { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private string GetHangfireUrl()
        {
            // 在 Aspire 環境中，使用 Service Discovery 解析 API 服務的 URL
            // 由於無法在前端直接取得動態端口，我們使用環境變數或配置
            var apiBaseUrl = Configuration["services:apiservice:http:0"] 
                ?? Configuration["services:apiservice:https:0"]
#pragma warning disable S1075 // URIs should not be hardcoded
                ?? "http://localhost:5390"; // fallback
#pragma warning restore S1075 // URIs should not be hardcoded

            return $"{apiBaseUrl}/hangfire";
        }
    }
}
