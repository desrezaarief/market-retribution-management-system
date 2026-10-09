using System.ComponentModel.DataAnnotations;

namespace RetribusiPasar.Web.Models.Entities;

public class MediaDistribution : BaseEntity
{
    [Required, StringLength(50)]
    [Display(Name = "No. Berita Acara")]
    public string DistributionNo { get; set; } = "";

    [Range(1, int.MaxValue)]
    [Display(Name = "Pasar Tujuan")]
    public int MarketId { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Jenis Media")]
    public string MediaType { get; set; } = "STRD";

    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Serah Terima")]
    public DateTime DistributionDate { get; set; } = DateTime.Today;

    [Range(0, 999999)]
    [Display(Name = "Seri Awal")]
    public int StartSerial { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Seri Akhir")]
    public int EndSerial { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Nama Penerima")]
    public string ReceiverName { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "No. Surat Permintaan")]
    public string? RequestLetterNo { get; set; }

    [StringLength(500)]
    [Display(Name = "Keterangan")]
    public string? Notes { get; set; }

    public int TotalSheets => EndSerial >= StartSerial ? EndSerial - StartSerial + 1 : 0;
}
