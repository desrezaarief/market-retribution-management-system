using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class RetributionUnit : BaseEntity
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar")]
    public int MarketId { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Blok / Unit")]
    public string Code { get; set; } = "";

    [Range(1, int.MaxValue)]
    [Display(Name = "Objek Retribusi")]
    public int RetributionTypeId { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Wajib Retribusi / Pedagang")]
    public string PayerName { get; set; } = "";

    [Display(Name = "Petugas")]
    public int? CollectorId { get; set; }

    [Required, StringLength(40)]
    [Display(Name = "Status Unit")]
    public string Status { get; set; } = "Berfungsi";

    [DataType(DataType.Date)]
    [Display(Name = "Mulai Ditagih")]
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
}
