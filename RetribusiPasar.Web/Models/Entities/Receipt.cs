using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class Receipt : BaseEntity
{
    [Required, StringLength(50)]
    [Display(Name = "No. Transaksi")]
    public string TransactionNo { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Penerimaan")]
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar")]
    public int MarketId { get; set; }

    [Display(Name = "Kios / Unit")]
    public int? UnitId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Jenis Retribusi")]
    public int RetributionTypeId { get; set; }

    [Display(Name = "Petugas")]
    public int? CollectorId { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Seri Awal")]
    public int? StartSerial { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Seri Akhir")]
    public int? EndSerial { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Periode Mulai")]
    public DateTime PeriodStart { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Range(1, 36)]
    [Display(Name = "Jumlah Bulan")]
    public int Months { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999999")]
    [Display(Name = "Nominal Seharusnya")]
    public decimal ExpectedAmount { get; set; }

    [Range(typeof(decimal), "1", "999999999999")]
    [Display(Name = "Nominal Diterima")]
    public decimal ReceivedAmount { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Metode Bayar")]
    public string PaymentMethod { get; set; } = "Tunai";

    [StringLength(100)]
    [Display(Name = "No. Referensi")]
    public string? PaymentReference { get; set; }

    [StringLength(500)]
    [Display(Name = "Keterangan")]
    public string? Notes { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [StringLength(300)]
    [Display(Name = "Alasan Hapus")]
    public string? DeleteReason { get; set; }
}
