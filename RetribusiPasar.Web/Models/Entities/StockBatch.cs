using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class StockBatch : BaseEntity
{
    [Required, StringLength(20)]
    [Display(Name = "Jenis Media")]
    public string MediaType { get; set; } = "STRD";

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Terima")]
    public DateTime ReceivedDate { get; set; } = DateTime.Today;

    [Range(0, 999999)]
    [Display(Name = "Seri Awal")]
    public int StartSerial { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Seri Akhir")]
    public int EndSerial { get; set; }

    [StringLength(100)]
    [Display(Name = "No. Dokumen / BA")]
    public string? DocumentNo { get; set; }

    [StringLength(500)]
    [Display(Name = "Keterangan")]
    public string? Notes { get; set; }

    public int TotalSheets => EndSerial >= StartSerial ? EndSerial - StartSerial + 1 : 0;
}
