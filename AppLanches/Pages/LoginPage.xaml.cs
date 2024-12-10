using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;

    public LoginPage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
    }

    private async void BtnSignIn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EntEmail.Text))
        {
            await DisplayAlert("Erro", "Informe o email", "Cancelar");
            return;
        }

        if (string.IsNullOrEmpty(EntPassword.Text))
        {
            await DisplayAlert("Erro", "Informe a senha", "Cancelar");
            return;
        }

        var response = await _apiService.Login(EntEmail.Text, EntPassword.Text);

        if (!response.HasError)
        {
            Application.Current!.MainPage = new AppShell(_apiService, _validator, _favoritesService);
        }
        else
        {
            await DisplayAlert("Erro", "Algo deu errado", "Cancelar");
        }
    }

    private async void TapRegister_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new InscricaoPage(_apiService, _validator, _favoritesService));
    }
}