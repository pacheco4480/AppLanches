namespace AppLanches.Pages;

public partial class EmptyCartPage : ContentPage
{
	public EmptyCartPage()
	{
		InitializeComponent();
	}

    private async void BtnReturn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}