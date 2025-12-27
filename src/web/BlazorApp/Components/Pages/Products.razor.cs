using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class Products : IDisposable
    {
        private Product[]? products;
        private bool isLoading = true;
        private string? errorMessage;
        private PersistingComponentStateSubscription _subscription;

        private ProductEditModel formModel = new();
        private bool isEditing = false;

        protected override async Task OnInitializedAsync()
        {
            _subscription = ApplicationState.RegisterOnPersisting(PersistProducts);

            if (ApplicationState.TryTakeFromJson<Product[]>("products", out var restored))
            {
                products = restored;
                isLoading = false;
            }
            else
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
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
        }

        private void CancelEdit()
        {
            ClearForm();
            isEditing = false;
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
                    CancelEdit();
                    Snackbar.Add("Product updated successfully", Severity.Success);
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
                        ClearForm();
                        Snackbar.Add("Product added successfully", Severity.Success);
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
                "Confirm Delete",
                $"Delete product #{id}? This action cannot be undone.",
                yesText: "Delete", cancelText: "Cancel");

            if (result != true) return;

            errorMessage = null;
            try
            {
                await ProductApiClient.DeleteProductAsync(id);

                products = (products ?? Array.Empty<Product>()).Where(p => p.Id != id).ToArray();

                if (isEditing && formModel.Id == id)
                {
                    CancelEdit();
                }

                Snackbar.Add("Product deleted successfully", Severity.Success);
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
