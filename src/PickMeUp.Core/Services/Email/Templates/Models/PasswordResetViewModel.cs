namespace PickMeUp.Core.Services.Email.Templates;

public class PasswordResetViewModel
{
    public string ResetUrl { get; set; } = default!;
    public int ExpirationHours { get; set; } = 1;
}
