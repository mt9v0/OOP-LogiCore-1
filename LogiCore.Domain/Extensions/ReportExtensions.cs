namespace LogiCore.Domain.Extensions;

using System.Text;

public static class ReportExtensions
{
    public static string ToReportTable<T>(this IEnumerable<T> items, string title, Func<T, string> formatter)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{title.ToUpperInvariant()}");
        int index = 1;
        foreach (var item in items)
        {
            sb.AppendLine($"{index++}. {formatter(item)}");
        }
        sb.AppendLine(new string('-', 40));
        return sb.ToString();
    }
}