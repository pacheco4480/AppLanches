using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class OrderDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;

    public OrderDetailsPage(int orderId,
                            decimal totalPrice,
                            ApiService apiService,
                            IValidator validator,
                            FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;

        LblTotalPrice.Text = totalPrice + " €";

        GetOrderDetails(orderId);
    }

    private async void GetOrderDetails(int orderId)
    {
        try
        {
            // Show loading indicator
            loadIndicator.IsRunning = true;
            loadIndicator.IsVisible = true;

            var (orderDetails, errorMessage) = await _apiService.GetOrderDetails(orderId);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return;
            }

            if (orderDetails is null)
            {
                await DisplayAlert("Error", errorMessage ?? "Couldn't retrieve order details.", "OK");
                return;
            }
            else
            {
                CvOrderDetails.ItemsSource = orderDetails;
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "An error ocurred trying to obtain details. Try again later.", "OK");
        }
        finally
        {
            // Hide loading indicator
            loadIndicator.IsRunning = false;
            loadIndicator.IsVisible = false;
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }
}