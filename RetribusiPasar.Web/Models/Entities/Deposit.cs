using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetribusiPasar.Web.Models.Entities;

public class Deposit : BaseEntity
{
    [Required, StringLength(50)]
    [Display(Name = "No. Setoran / STS")]
    public string DepositNo { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Setoran")]
    public DateTime DepositDate { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar")]
    public int MarketId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Dari Tanggal")]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Sampai Tanggal")]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [Required, StringLength(150)]
    [Display(Name = "Bank / Rekening Tujuan")]
    public string BankName { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Referensi Bank")]
    public string? ReferenceNo { get; set; }

    [Range(typeof(decimal), "1", "999999999999")]
    [Display(Name = "Jumlah Disetor")]
    public decimal Amount { get; set; }

    [StringLength(500)]
    [Display(Name = "Keterangan")]
    public string? Notes { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "VERIFIED";

    [NotMapped]
    public List<int> ReceiptIds { get; set; } = new();
}
