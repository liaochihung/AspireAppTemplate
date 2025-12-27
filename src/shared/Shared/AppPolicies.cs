namespace AspireAppTemplate.Shared;

public static class AppPolicies
{
    // 定義「管理產品」的策略名稱 (包含 Create, Update, Delete)
    public const string CanManageProducts = "CanManageProducts";
    
    // 定義「查看天氣」的策略名稱
    public const string CanViewWeather = "CanViewWeather";

    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageRoles = "CanManageRoles";
}
