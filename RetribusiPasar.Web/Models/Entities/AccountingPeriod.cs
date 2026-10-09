using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class AccountingPeriod : BaseEntity
{
    [Required, RegularExpression(@"^\d{4}-\d{2}$")]
    [Display(Name = "Periode")]
    public string Period { get; set; } = DateTime.Today.ToString("yyyy-MM");

    [Required, StringLength(20)]
    public string Status { get; set; } = "OPEN";

    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }

    [StringLength(300)]
    [Display(Name = "Alasan Reopen")]
    public string? ReopenReason { get; set; }
}
