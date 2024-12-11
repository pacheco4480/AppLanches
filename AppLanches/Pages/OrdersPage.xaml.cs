using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;

    public OrdersPage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetOrdersList();
    }

    private async Task GetOrdersList()
    {
        try
        {
            // Show loading indicator
            loadOrdersIndicator.IsRunning = true;
            loadOrdersIndicator.IsVisible = true;

            var (orders, errorMessage) = await _apiService.GetOrdersByUser(Preferences.Get("userid", 0));

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return;
            }
            if (errorMessage == "NotFound")
            {
                await DisplayAlert("Warning", "You have no orders.", "OK");
                return;
            }
            if (orders is null)
            {
                await DisplayAlert("Error", errorMessage ?? "NCouldn't retrieve orders.", "OK");
                return;
            }
            else
            {
                CvOrders.ItemsSource = orders;
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "An error ocurred trying to obtain the orders. Try again later.", "OK");
        }
        finally
        {
            // Hide loading indicator
            loadOrdersIndicator.IsRunning = false;
            loadOrdersIndicator.IsVisible = false;
        }
    }

    private void CvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.CurrentSelection.FirstOrDefault() as OrderByUser;

        if (selectedItem == null) return;

        Navigation.PushAsync(new OrderDetailsPage(selectedItem.Id,
                                                  selectedItem.Total,
                                                  _apiService,
                                                  _validator,
                                                  _favoritesService));

        ((CollectionView)sender).SelectedItem = null;
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }
}