namespace AppLanches.Models;

public class Token
{
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
}
