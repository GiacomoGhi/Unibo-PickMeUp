using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Features.Login;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = default!;

    [Display(Name = "Rimani connesso")]
    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; } = default!;
}
