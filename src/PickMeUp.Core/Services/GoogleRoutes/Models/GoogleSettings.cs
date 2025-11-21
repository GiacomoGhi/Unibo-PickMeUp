namespace PickMeUp.Core.Services.GoogleRoutes;

public class GoogleSettings
{
    /// <summary>
    /// Google OAuth Client ID.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Google API Key for accessing Google services.
    /// </summary>
    public string MapsApiKey { get; set; } = default!;
}