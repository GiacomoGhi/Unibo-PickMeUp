using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Features.Login;

public class ResendConfirmationViewModel
{
    [Required(ErrorMessage = "L'email è obbligatoria")]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
    public string Email { get; set; } = default!;
}
