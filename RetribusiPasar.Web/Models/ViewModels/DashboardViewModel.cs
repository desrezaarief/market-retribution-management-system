namespace RetribusiPasar.Web.Models.ViewModels;

public class DashboardViewModel
{
    public decimal TotalReceipt { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal OutstandingDeposit => TotalReceipt - TotalDeposited;
    public decimal TotalArrears { get; set; }
    public int UsedMedia { get; set; }
    public List<MarketSummaryRow> Markets { get; set; } = new();
}

public class MarketSummaryRow
{
    public int MarketId { get; set; }
    public string MarketName { get; set; } = "";
    public decimal Receipt { get; set; }
    public decimal Deposit { get; set; }
    public decimal Outstanding => Receipt - Deposit;
    public decimal Arrears { get; set; }
}
