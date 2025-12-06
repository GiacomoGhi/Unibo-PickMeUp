using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Models.Auth
{
    public class GoogleLoginRequest
    {
        public string Credential { get; set; } = default!;
    }

    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool RememberMe { get; set; }
        public bool ShowPasswordResetModal { get; set; }
        public ResetPasswordViewModel? ResetPasswordData { get; set; }
    }

    public class SignUpViewModel
    {
        public string? ReturnUrl { get; set; }
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        public string ConfirmPassword { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }

    public class ResendConfirmationViewModel
    {
        public string Email { get; set; } = default!;
    }

    public class RequestPasswordResetViewModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserisci un'email valida")]
        public string Email { get; set; } = default!;
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        
        public string Token { get; set; } = default!;
        
        [Required(ErrorMessage = "La password è obbligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere lunga almeno 8 caratteri")]
        [DataType(DataType.Password)]
        [Display(Name = "Nuova Password")]
        public string NewPassword { get; set; } = default!;
        
        [Required(ErrorMessage = "La conferma password è obbligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Conferma Password")]
        [Compare("NewPassword", ErrorMessage = "Le password non corrispondono")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
