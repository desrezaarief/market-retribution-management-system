using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetribusiPasar.Web.Models.Entities;

public class AppUser : BaseEntity
{
    [Required, StringLength(80)]
    public string Username { get; set; } = "";

    [Required, StringLength(150)]
    [Display(Name = "Nama")]
    public string DisplayName { get; set; } = "";

    [Required, StringLength(30)]
    public string Role { get; set; } = "Operator";

    [Display(Name = "Pasar")]
    public int? MarketId { get; set; }

    public string PasswordHash { get; set; } = "";

    [NotMapped, DataType(DataType.Password)]
    [Display(Name = "Password Baru")]
    public string? NewPassword { get; set; }
}
