namespace Library.Presentation.Authentication;

public class JwtSettings
{
    public bool Enabled { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string SigningKey { get; set; }
    public TimeSpan ExpirationDuration { get; set; }
}
