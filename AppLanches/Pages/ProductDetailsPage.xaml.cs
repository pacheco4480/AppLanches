using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class ProductDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private int _productId;
    private string? _urlImage;
    private bool _loginPageDisplayed = false;

    public ProductDetailsPage(int productId, string productName, ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
        _productId = productId;
        Title = productName ?? "Product Details";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetProductDetails(_productId);
        UpdateFavoriteButton();
    }

    private async Task<Product?> GetProductDetails(int productId)
    {
        var (productDetail, errorMessage) = await _apiService.GetProductDetails(productId);

        if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
        {
            await DisplayLoginPage();
            return null;
        }

        if (productDetail is null)
        {
            await DisplayAlert("Error", errorMessage ?? "Couldn't get product.", "OK");
            return null;
        }

        if (productDetail != null)
        {
            ProductImage.Source = productDetail.ImagePath;
            LblProductName.Text = productDetail.Name;
            LblProductPrice.Text = productDetail.Price.ToString();
            LblProductDescription.Text = productDetail.Details;
            LblTotalPrice.Text = productDetail.Price.ToString();
            _urlImage = productDetail.ImagePath;
        }
        else
        {
            await DisplayAlert("Error", errorMessage ?? "Couldn't get product details.", "OK");
            return null;
        }
        return productDetail;
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }

    private async void BtnFavoriteImage_Clicked(object sender, EventArgs e)
    {
        try
        {
            var existsFavorite = await _favoritesService.ReadAsync(_productId);
            if (existsFavorite is not null)
            {
                await _favoritesService.DeleteAsync(existsFavorite);
            }
            else
            {
                var favoriteProduct = new FavoriteProduct()
                {
                    ProductId = _productId,
                    IsFavorite = true,
                    Details = LblProductDescription.Text,
                    Name = LblProductName.Text,
                    Price = Convert.ToDecimal(LblProductPrice.Text),
                    UrlImage = _urlImage
                };

                await _favoritesService.CreateAsync(favoriteProduct);
            }

            UpdateFavoriteButton();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
        }
    }

    private async void UpdateFavoriteButton()
    {
        var existsFavorite = await
               _favoritesService.ReadAsync(_productId);

        if (existsFavorite is not null)
            BtnFavoriteImage.Source = "heartfill";
        else
            BtnFavoriteImage.Source = "heart";
    }

    private void BtnRemove_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantity.Text, out int quantity) &&
            decimal.TryParse(LblProductPrice.Text, out decimal unitPrice))
        {
            quantity = Math.Max(1, quantity - 1);
            LblQuantity.Text = quantity.ToString();

            var totalPrice = quantity * unitPrice;
            LblTotalPrice.Text = totalPrice.ToString();
        }
        else
        {
            DisplayAlert("Error", "Invalid values.", "OK");
        }
    }

    private void BtnAdd_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantity.Text, out int quantity) &&
            decimal.TryParse(LblProductPrice.Text, out decimal unitPrice))
        {
            quantity++;
            LblQuantity.Text = quantity.ToString();

            var totalPrice = quantity * unitPrice;
            LblTotalPrice.Text = totalPrice.ToString(); 
        }
        else
        {
            DisplayAlert("Error", "Invalid values.", "OK");
        }
    }

    private async void BtnAddToCart_Clicked(object sender, EventArgs e)
    {
        try
        {
            var purchaseCart = new PurchaseCart()
            {
                Quantity = Convert.ToInt32(LblQuantity.Text),
                UnitPrice = Convert.ToDecimal(LblProductPrice.Text),
                Total = Convert.ToDecimal(LblTotalPrice.Text),
                ProductId = _productId,
                ClientId = Preferences.Get("userid", 0)
            };
            var response = await _apiService.AddItemToCart(purchaseCart);
            if (response.Data)
            {
                await DisplayAlert("Success", "Item added to cart !", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", $"Failed adding item: {response.ErrorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
        }
    }
}