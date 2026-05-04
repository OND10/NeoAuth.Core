namespace Auth.Application.Configuration;

public class AuthOptions
{
    public const string SectionName = "AuthModule";

    // JWT Settings
    public string JwtSecret { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "AuthModule";
    public string JwtAudience { get; set; } = "AuthModuleClients";
    public int AccessTokenExpirationMinutes { get; set; } = 30;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int ClientTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// When true, include permissions/scopes directly in the JWT.
    /// When false (default), only sub, tenant_id, roles, client_id are in the JWT.
    /// Permissions are loaded at runtime via middleware.
    /// </summary>
    public bool UseEnrichedTokens { get; set; } = false;

    // Database
    public string ConnectionString { get; set; } = string.Empty;

    // Google OAuth
    public GoogleOAuthOptions Google { get; set; } = new();

    // Cache
    public int CacheExpirationMinutes { get; set; } = 15;
    public string FrontendUrl { get; set; } = string.Empty;
}

public class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
