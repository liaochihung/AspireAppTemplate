using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class Products : IDisposable
    {
        [Inject]
        private IAuthorizationService AuthorizationService { get; set; } = default!;

        private Product[]? products;
        private bool isLoading = true;
        private string? errorMessage;
        private PersistingComponentStateSubscription _subscription;
        private string _searchString = string.Empty;
        private bool showForm = false;
        private bool _canManage = false;

        private ProductEditModel formModel = new();
        private bool isEditing = false;

        private Func<Product, bool> _quickFilter => product =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (product.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (product.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            _subscription = ApplicationState.RegisterOnPersisting(PersistProducts);

            // Check authorization
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var authResult = await AuthorizationService.AuthorizeAsync(user, AppPolicies.CanManageProducts);
                _canManage = authResult.Succeeded;

                if (ApplicationState.TryTakeFromJson<Product[]>("products", out var restored))
                {
                    products = restored;
                    isLoading = false;
                }
                else
                {
                    await LoadProducts();
                }
            }
        }

        private Task PersistProducts()
        {
            ApplicationState.PersistAsJson("products", products);
            return Task.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            _subscription.Dispose();
        }

        private async Task LoadProducts()
        {
            isLoading = true;
            errorMessage = null;
            try
            {
                products = await ProductApiClient.GetProductsAsync();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenAddForm()
        {
            formModel = new ProductEditModel();
            isEditing = false;
            showForm = true;
        }

        private void StartEdit(Product p)
        {
            formModel = new ProductEditModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description
            };
            isEditing = true;
            showForm = true;
        }

        private void CancelEdit()
        {
            ClearForm();
            isEditing = false;
        }

        private void CloseForm()
        {
            showForm = false;
            isEditing = false;
            ClearForm();
        }

        private void ClearForm()
        {
            formModel = new ProductEditModel();
        }

        private async Task SaveForm()
        {
            errorMessage = null;
            try
            {
                if (isEditing)
                {
                    var toUpdate = new Product
                    {
                        Id = formModel.Id,
                        Name = formModel.Name ?? string.Empty,
                        Price = formModel.Price,
                        Description = formModel.Description
                    };

                    await ProductApiClient.UpdateProductAsync(toUpdate.Id, toUpdate);

                    var arr = (products ?? Array.Empty<Product>()).ToList();
                    var idx = arr.FindIndex(x => x.Id == toUpdate.Id);
                    if (idx >= 0)
                    {
                        arr[idx] = toUpdate;
                        products = arr.ToArray();
                    }
                    CloseForm();
                    Snackbar.Add("產品更新成功", Severity.Success);
                }
                else
                {
                    var toCreate = new Product
                    {
                        Name = formModel.Name ?? string.Empty,
                        Price = formModel.Price,
                        Description = formModel.Description
                    };

                    var created = await ProductApiClient.CreateProductAsync(toCreate);
                    if (created is not null)
                    {
                        var list = (products ?? Array.Empty<Product>()).ToList();
                        list.Add(created);
                        products = list.ToArray();
                        CloseForm();
                        Snackbar.Add("產品新增成功", Severity.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task ConfirmDelete(int id)
        {
            var result = await DialogService.ShowMessageBox(
                "確認刪除",
                $"確定要刪除產品 #{id} 嗎？此操作無法復原。",
                yesText: "刪除", cancelText: "取消");

            if (result != true) return;

            errorMessage = null;
            try
            {
                await ProductApiClient.DeleteProductAsync(id);

                products = (products ?? Array.Empty<Product>()).Where(p => p.Id != id).ToArray();

                if (isEditing && formModel.Id == id)
                {
                    CloseForm();
                }

                Snackbar.Add("產品刪除成功", Severity.Success);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private sealed class ProductEditModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public decimal Price { get; set; }
            public string? Description { get; set; }
        }
    }
}
