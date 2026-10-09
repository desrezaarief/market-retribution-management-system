namespace RetribusiPasar.Web.Models.ViewModels;

public class ReportViewModel
{
    public string Period { get; set; } = DateTime.Today.ToString("yyyy-MM");
    public int? MarketId { get; set; }
    public List<ReportRow> Rows { get; set; } = new();
    public decimal TotalReceipt => Rows.Sum(x => x.Receipt);
    public decimal TotalDeposit => Rows.Sum(x => x.Deposit);
    public decimal TotalOutstanding => Rows.Sum(x => x.Outstanding);
}

public class ReportRow
{
    public string MarketName { get; set; } = "";
    public decimal Receipt { get; set; }
    public decimal Deposit { get; set; }
    public decimal Outstanding => Receipt - Deposit;
}
