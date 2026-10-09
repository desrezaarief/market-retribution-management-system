using System.Globalization;

namespace RetribusiPasar.Web.Helpers;

public static class FormatHelper
{
    private static readonly CultureInfo Id = CultureInfo.GetCultureInfo("id-ID");

    public static string Rupiah(decimal value) => $"Rp{value.ToString("N0", Id)}";
    public static string Number(decimal value) => value.ToString("N0", Id);
    public static string Number(int value) => value.ToString("N0", Id);
}
