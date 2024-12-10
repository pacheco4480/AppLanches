using AppLanches.Pages;
using AppLanches.Services;
using AppLanches.Validators;


namespace AppLanches;

public partial class App : Application
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;

    public App(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
        SetMainPage();
    }

    private void SetMainPage()
    {
        var accessToken = Preferences.Get("accesstoken", string.Empty);

        if (string.IsNullOrEmpty(accessToken))
        {
            MainPage = new NavigationPage(new LoginPage(_apiService, _validator, _favoritesService));
            return;
        }

        MainPage = new AppShell(_apiService, _validator, _favoritesService);
    }
}