namespace RetribusiPasar.Web.Models.ViewModels;

public class TrackingViewModel
{
    public int? Serial { get; set; }
    public string? MediaType { get; set; }
    public string? Status { get; set; }
    public List<TrackingEvent> Events { get; set; } = new();
    public string? Error { get; set; }
}

public class TrackingEvent
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}
