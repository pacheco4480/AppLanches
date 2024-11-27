namespace AppLanches.Models;

public class Token
{
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
}
