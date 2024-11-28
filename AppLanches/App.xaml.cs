using AppLanches.Pages;
using AppLanches.Services;
using AppLanches.Validators;


namespace AppLanches;

public partial class App : Application
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;

    public App(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        MainPage = new NavigationPage(new InscricaoPage(_apiService, _validator));
    }
}

