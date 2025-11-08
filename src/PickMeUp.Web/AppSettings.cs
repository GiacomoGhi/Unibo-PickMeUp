namespace PickMeUp.Web;

public class AppSettings
{
    public bool DisabilitaControlloPassword { get; set; }
    
    /// <summary>
    /// Base URL of the application (e.g., https://pickmeup.example.com)
    /// </summary>
    public string BaseUrl { get; set; } = default!;
}
