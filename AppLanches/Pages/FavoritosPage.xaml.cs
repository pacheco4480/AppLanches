using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class FavoritosPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;

    public FavoritosPage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetFavoriteProducts();
    }

    private async Task GetFavoriteProducts()
    {
        try
        {
            var favoriteProducts = await _favoritesService.ReadAllAsync();

            if (favoriteProducts is null || favoriteProducts.Count == 0)
            {
                CvProducts.ItemsSource = null;
                LblWarning.IsVisible = true;
            }
            else
            {
                CvProducts.ItemsSource = favoriteProducts;
                LblWarning.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
        }
    }

    private void CvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as FavoriteProduct;

        if (currentSelection == null) return;

        Navigation.PushAsync(new ProductDetailsPage(currentSelection.ProductId,
                                                     currentSelection.Name!,
                                                     _apiService, _validator, _favoritesService));

        ((CollectionView)sender).SelectedItem = null;
    }
}