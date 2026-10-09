using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Ingat saya")]
    public bool RememberMe { get; set; }
}
