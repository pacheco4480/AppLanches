using AppLanches.Pages;
using AppLanches.Services;
using AppLanches.Validators;

namespace AppLanches
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _apiService;
        private readonly IValidator _validator;
        private readonly FavoritesService _favoritesService;

        public AppShell(ApiService apiService, IValidator validator, FavoritesService favoritesService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _validator = validator;
            _favoritesService = favoritesService;
            ConfigureShell();
        }

        private void ConfigureShell()
        {
            var homePage = new HomePage(_apiService, _validator, _favoritesService);
            var carrinhoPage = new CarrinhoPage(_apiService, _validator, _favoritesService);
            var favoritosPage = new FavoritosPage(_apiService, _validator, _favoritesService);
            var perfilPage = new ProfilePage(_apiService, _validator, _favoritesService);

            Items.Add(new TabBar
            {
                Items =
            {
                new ShellContent { Title = "Home",Icon = "home",Content = homePage  },
                new ShellContent { Title = "Carrinho", Icon = "cart",Content = carrinhoPage },
                new ShellContent { Title = "Favoritos",Icon = "heart",Content = favoritosPage },
                new ShellContent { Title = "Perfil",Icon = "profile",Content = perfilPage }
            }
            });
        }
    }

}
