using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class Market : BaseEntity
{
    [Required, StringLength(20)]
    [Display(Name = "Kode")]
    public string Code { get; set; } = "";

    [Required, StringLength(150)]
    [Display(Name = "Nama Pasar")]
    public string Name { get; set; } = "";

    [Required, StringLength(30)]
    [Display(Name = "Tipe")]
    public string Type { get; set; } = "Pasar";

    [StringLength(300)]
    [Display(Name = "Alamat")]
    public string? Address { get; set; }
}
