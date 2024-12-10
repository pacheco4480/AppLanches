using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;
    private bool _isDataLoaded = false;

    public HomePage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        LblUserName.Text = "Hello, " + Preferences.Get("username", string.Empty);
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
        Title = AppConfig.HomePageTitle;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_isDataLoaded)
        {
            await LoadDataAsync();
            _isDataLoaded = true;
        }
    }

    private async Task LoadDataAsync()
    {
        var categoriesTask = GetCategoriesList();
        var bestSellersTask = GetBestSellers();
        var popularsTask = GetPopulars();

        await Task.WhenAll(categoriesTask, bestSellersTask, popularsTask);
    }

    private async Task<IEnumerable<Category>> GetCategoriesList()
    {
        try
        {
            var (categories, errorMessage) = await _apiService.GetCategories();

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Category>();
            }

            if (categories == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Unable to retrieve categories.", "OK");
                return Enumerable.Empty<Category>();
            }

            CvCategories.ItemsSource = categories;
            return categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
            return Enumerable.Empty<Category>();
        }
    }

    private async Task<IEnumerable<Product>> GetBestSellers()
    {
        try
        {
            var (products, errorMessage) = await _apiService.GetProducts("bestseller", string.Empty);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (products == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Unable to retrieve categories.", "OK");
                return Enumerable.Empty<Product>();
            }

            CvBestSellers.ItemsSource = products;
            return products;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
            return Enumerable.Empty<Product>();
        }
    }

    private async Task<IEnumerable<Product>> GetPopulars()
    {
        try
        {
            var (products, errorMessage) = await _apiService.GetProducts("popular", string.Empty);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (products == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Unable to retrieve categories.", "OK");
                return Enumerable.Empty<Product>();
            }

            CvPopulars.ItemsSource = products;
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

    private void CvCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Category;

        if (currentSelection == null) return;


        Navigation.PushAsync(new ProductsListPage(_apiService,
                                                  _validator,
                                                  currentSelection.Name!,
                                                  currentSelection.Id,
                                                  _favoritesService
                                                  ));

        ((CollectionView)sender).SelectedItem = null;
    }

    private void CvBestSellers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            NavigateToProductDetailsPage(collectionView, e);
        }
    }

    private void CvPopulars_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            NavigateToProductDetailsPage(collectionView, e);
        }
    }

    private void NavigateToProductDetailsPage(CollectionView collectionView, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

        if (currentSelection == null)
            return;

        Navigation.PushAsync(new ProductDetailsPage(
                                 currentSelection.Id, currentSelection.Name!, _apiService, _validator, _favoritesService
        ));

        collectionView.SelectedItem = null;
    }


}