using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class ProductsListPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private int _categoryId;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;

    public ProductsListPage(ApiService apiService, IValidator validator, string categoryName, int categoryId, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _categoryId = categoryId;
        _favoritesService = favoritesService;
        Title = categoryName ?? "Products";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetProductsList(_categoryId);
    }

    private async Task<IEnumerable<Product>> GetProductsList(int categoryId)
    {
        try
        {
            var (products, errorMessage) = await _apiService.GetProducts("category", categoryId.ToString());

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (products is null)
            {
                await DisplayAlert("Error", errorMessage ?? "Couldn't get categories.", "OK");
                return Enumerable.Empty<Product>();
            }

            CvProducts.ItemsSource = products;
            return products;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
            return Enumerable.Empty<Product>();
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }

    private void CvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

        if (currentSelection is null)
            return;

        Navigation.PushAsync(new ProductDetailsPage(currentSelection.Id,
                                                     currentSelection.Name!,
                                                     _apiService,
                                                     _validator,
                                                     _favoritesService));

        ((CollectionView)sender).SelectedItem = null;
    }
}