using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class Collector : BaseEntity
{
    [Required, StringLength(30)]
    [Display(Name = "Kode/NIP")]
    public string Code { get; set; } = "";

    [Required, StringLength(120)]
    [Display(Name = "Nama Petugas")]
    public string Name { get; set; } = "";

    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar")]
    public int MarketId { get; set; }

    [StringLength(30)]
    [Display(Name = "No. HP")]
    public string? Phone { get; set; }
}
