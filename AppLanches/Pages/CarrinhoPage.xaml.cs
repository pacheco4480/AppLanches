using AppLanches.Services;
using AppLanches.Models;
using System.Collections.ObjectModel;
using AppLanches.Validators;


namespace AppLanches.Pages;

public partial class CarrinhoPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;
    private bool _isNavigatingToEmptyCartPage = false;

    private ObservableCollection<PurchaseCartItem> PurchaseCartItems = new ObservableCollection<PurchaseCartItem>();

    public CarrinhoPage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (IsNavigatingToEmptyCartPage()) return;


        bool hasItems = await GetPurchaseCartItems();

        if (hasItems)
        {
            ShowAddress();
        }
        else
        {
            await NavigateToEmptyCart();
        }
    }

    private async Task NavigateToEmptyCart()
    {
        LblAddress.Text = string.Empty;
        _isNavigatingToEmptyCartPage = true;
        await Navigation.PushAsync(new EmptyCartPage());
    }

    private void ShowAddress()
    {
        bool savedAddress = Preferences.ContainsKey("address");

        if (savedAddress)
        {
            string name = Preferences.Get("name", string.Empty);
            string address = Preferences.Get("address", string.Empty);
            string phone = Preferences.Get("phone", string.Empty);

            LblAddress.Text = $"{name}\n{address}\n{phone}";
        }
        else
        {
            LblAddress.Text = "Type your address";
        }
    }

    private bool IsNavigatingToEmptyCartPage()
    {
        if (_isNavigatingToEmptyCartPage)
        {
            _isNavigatingToEmptyCartPage = false;
            return true;
        }
        return false;
    }

    private async Task<bool> GetPurchaseCartItems()
    {
        try
        {
            var userId = Preferences.Get("userid", 0);
            var (purchaseCartItems, errorMessage) = await
                     _apiService.GetPurchaseCartItems(userId);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                // Redirecionar para a p?gina de login
                await DisplayLoginPage();
                return false;
            }

            if (purchaseCartItems == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Couldn't obtain the items from the purchase cart.", "OK");
                return false;
            }

            PurchaseCartItems.Clear();
            foreach (var item in purchaseCartItems)
            {
                PurchaseCartItems.Add(item);
            }

            CvCart.ItemsSource = PurchaseCartItems;
            UpdateTotalPrice();

            if (!PurchaseCartItems.Any())
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "OK");
            return false;
        }
    }

    private void UpdateTotalPrice()
    {
        try
        {
            var totalPrice = PurchaseCartItems.Sum(item => item.Price * item.Quantity);
            LblTotalPrice.Text = totalPrice.ToString();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error ocurred updating total price: {ex.Message}", "OK");
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }

    private async void BtnAdd_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PurchaseCartItem cartItem)
        {
            cartItem.Quantity++;
            UpdateTotalPrice();
            await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "increase");
        }
    }

    private async void BtnDecrease_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PurchaseCartItem cartItem)
        {
            if (cartItem.Quantity == 1) return;
            else
            {
                cartItem.Quantity--;
                UpdateTotalPrice();
                await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "decrease");
            }
        }
    }

    private async void BtnDelete_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is PurchaseCartItem cartItem)
        {
            bool response = await DisplayAlert("Confirm",
                          "Delete item from the purhase cart?", "Yes", "No");
            if (response)
            {
                PurchaseCartItems.Remove(cartItem);
                UpdateTotalPrice();
                await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "delete");
            }
        }
    }

    private void BtnEditAddress_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AddressPage());
    }

    private async void TapConfirmOrder_Tapped(object sender, TappedEventArgs e)
    {
        if (PurchaseCartItems == null || !PurchaseCartItems.Any())
        {
            await DisplayAlert("Info", "Empty cart or order already confirmed.", "OK");
            return;
        }

        var order = new Order()
        {
            Address = LblAddress.Text,
            UserId = Preferences.Get("userid", 0),
            Total = Convert.ToDecimal(LblTotalPrice.Text)
        };

        var response = await _apiService.ConfirmOrder(order);

        if (response.HasError)
        {
            if (response.ErrorMessage == "Unauthorized")
            {
                // Redirecionar para a p gina de login
                await DisplayLoginPage();
                return;
            }
            await DisplayAlert("Ups !!!", $"Something went wrong: {response.ErrorMessage}", "Cancel");
            return;
        }

        PurchaseCartItems.Clear();
        LblAddress.Text = "Type your address";
        LblTotalPrice.Text = "0.00";

        await Navigation.PushAsync(new OrderConfirmedPage());
    }
}