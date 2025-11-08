namespace PickMeUp.Core.Services.Email.Templates;

public class EmailConfirmationViewModel
{
    public string ConfirmationUrl { get; set; } = default!;
    public int ExpirationHours { get; set; } = 24;
}
