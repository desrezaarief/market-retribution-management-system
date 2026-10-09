namespace RetribusiPasar.Web.Models.ViewModels;

public class ReceiptFormContextViewModel
{
    public int? UnitId { get; set; }
    public int MarketId { get; set; }
    public string MarketName { get; set; } = "";
    public int RetributionTypeId { get; set; }
    public string RetributionTypeName { get; set; } = "";
    public int? CollectorId { get; set; }
    public string CollectorName { get; set; } = "";
    public string MediaType { get; set; } = "";
    public decimal ExpectedAmount { get; set; }
    public decimal Tariff { get; set; }
    public List<SerialRangeViewModel> AvailableRanges { get; set; } = new();
}

public class SerialRangeViewModel
{
    public int StartSerial { get; set; }
    public int EndSerial { get; set; }
    public int TotalSheets => EndSerial - StartSerial + 1;
    public string Label => StartSerial == EndSerial
        ? $"{StartSerial:000000}"
        : $"{StartSerial:000000}-{EndSerial:000000}";
}
