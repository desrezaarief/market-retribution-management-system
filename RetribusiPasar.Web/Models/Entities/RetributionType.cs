using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class RetributionType : BaseEntity
{
    [Required, StringLength(20)]
    [Display(Name = "Kode")]
    public string Code { get; set; } = "";

    [Required, StringLength(100)]
    [Display(Name = "Jenis Retribusi")]
    public string Name { get; set; } = "";

    [Required, StringLength(20)]
    [Display(Name = "Media")]
    public string MediaType { get; set; } = "STRD";

    [Required, StringLength(30)]
    [Display(Name = "Siklus Tagihan")]
    public string BillingCycle { get; set; } = "Bulanan";
}
