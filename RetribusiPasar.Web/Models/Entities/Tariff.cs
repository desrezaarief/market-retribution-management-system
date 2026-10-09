using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class Tariff : BaseEntity
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar")]
    public int MarketId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Jenis Retribusi")]
    public int RetributionTypeId { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    [Display(Name = "Tarif")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Berlaku Mulai")]
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Berlaku Sampai")]
    public DateTime? EffectiveTo { get; set; }
}
