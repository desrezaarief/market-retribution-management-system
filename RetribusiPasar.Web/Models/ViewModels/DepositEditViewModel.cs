using Microsoft.AspNetCore.Mvc.Rendering;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Models.ViewModels;

public class DepositEditViewModel
{
    public Deposit Deposit { get; set; } = new();
    public List<int> SelectedReceiptIds { get; set; } = new();
    public List<ReceiptChoice> ReceiptChoices { get; set; } = new();
    public IEnumerable<SelectListItem> Markets { get; set; } = Array.Empty<SelectListItem>();
}

public class ReceiptChoice
{
    public int Id { get; set; }
    public string TransactionNo { get; set; } = "";
    public DateTime ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public bool Selected { get; set; }
}
