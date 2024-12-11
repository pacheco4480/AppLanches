using AppLanches.Services;
using AppLanches.Validators;
using AppLanches.Models;


namespace AppLanches.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private readonly FavoritesService _favoritesService;
    private bool _loginPageDisplayed = false;

    public ProfilePage(ApiService apiService, IValidator validator, FavoritesService favoritesService)
    {
        InitializeComponent();
        LblUserName.Text = Preferences.Get("username", string.Empty);
        _apiService = apiService;
        _validator = validator;
        _favoritesService = favoritesService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ImgBtnProfile.Source = await GetProfileImage();
    }

    private async Task<string?> GetProfileImage()
    {
        string defaultImage = AppConfig.DefaultProfileImage;

        var (response, errorMessage) = await _apiService.GetUserProfileImage();

        if (errorMessage is not null)
        {
            switch (errorMessage)
            {
                case "Unauthorized":
                    if (!_loginPageDisplayed)
                    {
                        await DisplayLoginPage();
                        return null;
                    }
                    break;
                default:
                    await DisplayAlert("Erro", errorMessage ?? "Couldn't retrieve image.", "OK");
                    return defaultImage;
            }
        }

        if (response?.UrlImage is not null)
        {
            return response.ImagePath;
        }

        return defaultImage;
    }

    private async Task<byte[]?> SelectImageAsync()
    {
        try
        {
            var archive = await MediaPicker.PickPhotoAsync();

            if (archive is null) return null;

            using (var stream = await archive.OpenReadAsync())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "The functionality is not supported on the device.", "Ok");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Error", "Permissions not granted to access the camera or gallery.", "Ok");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error selecting image: {ex.Message}", "Ok");
        }
        return null;
    }

    private async void ImgBtnProfile_Clicked(object sender, EventArgs e)
    {
        try
        {
            var imageArray = await SelectImageAsync();
            if (imageArray is null)
            {
                await DisplayAlert("Error", "Unable to load the image.", "Ok");
                return;
            }
            ImgBtnProfile.Source = ImageSource.FromStream(() => new MemoryStream(imageArray));

            var response = await _apiService.UploadUserImage(imageArray);
            if (response.Data)
            {
                await DisplayAlert("", "Image loaded successfully", "Ok");
            }
            else
            {
                await DisplayAlert("Error", response.ErrorMessage ?? "Unknown error ocurred", "Cancel");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error ocurred: {ex.Message}", "Ok");
        }
    }

    private void TapOrders_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new OrdersPage(_apiService, _validator, _favoritesService));
    }

    private void TapMyAccount_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new MyAccountPage(_apiService));
    }

    private void TapQuestions_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new QuestionsPage());
    }

    private void BtnLogout_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("accesstoken", string.Empty);
        Application.Current!.MainPage = new NavigationPage(new LoginPage(_apiService, _validator, _favoritesService));
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator, _favoritesService));
    }
}