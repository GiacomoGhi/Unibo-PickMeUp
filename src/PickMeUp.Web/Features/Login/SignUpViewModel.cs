using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Features.Login;

public class SignUpViewModel
{
    [Required(ErrorMessage = "L'email è obbligatoria")]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "La password è obbligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere di almeno 8 caratteri")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "La conferma password è obbligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Conferma Password")]
    [Compare("Password", ErrorMessage = "Le password non corrispondono")]
    public string ConfirmPassword { get; set; } = default!;

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [Display(Name = "Nome")]
    [StringLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Il cognome è obbligatorio")]
    [Display(Name = "Cognome")]
    [StringLength(50, ErrorMessage = "Il cognome non può superare i 50 caratteri")]
    public string LastName { get; set; } = default!;

    public string ReturnUrl { get; set; } = default!;
}
