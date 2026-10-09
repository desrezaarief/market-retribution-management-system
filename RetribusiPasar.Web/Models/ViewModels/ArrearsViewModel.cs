namespace RetribusiPasar.Web.Models.ViewModels;

public class ArrearsViewModel
{
    public string Period { get; set; } = DateTime.Today.ToString("yyyy-MM");
    public int? MarketId { get; set; }
    public List<ArrearsRow> Rows { get; set; } = new();
    public int TotalUnits => Rows.Count;
    public int TotalMonths => Rows.Sum(x => x.Months);
    public decimal TotalAmount => Rows.Sum(x => x.Amount);
}

public class ArrearsRow
{
    public string MarketName { get; set; } = "";
    public string UnitCode { get; set; } = "";
    public string PayerName { get; set; } = "";
    public string CollectorName { get; set; } = "";
    public int Months { get; set; }
    public decimal Tariff { get; set; }
    public decimal Amount => Months * Tariff;
}
