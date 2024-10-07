using System.Text;
using TotkCommon.Components;

namespace Tkmm.Core.Helpers;

public static class DumpIntegrityHelper
{
    public static ValueTask<ArraySegment<byte>> GetChecksumTable()
    {
        throw new NotImplementedException();
    }
    
    public static string FormatResults(TotkDumpResults results)
    {
        StringBuilder sb = new();
        
        return results.IsCompleteDump switch {
            true => FormatResultsSuccess(sb, results),
            false => FormatResultsFailure(sb, results),
        };
    }
    
    public static string FormatResultsSuccess(StringBuilder sb, TotkDumpResults results)
    {
        sb.AppendLine("[Complete dump]");

        if (results.ExtraFiles.Count > 0) {
            sb.Append("Extra files: ");
            sb.AppendLine(results.ExtraFiles.FormatStringCollection());
        }
        
        return sb.ToString();
    }
    
    private static string FormatResultsFailure(StringBuilder sb, TotkDumpResults results)
    {
        sb.AppendLine("[Incomplete or invalid dump]");
        
        if (results.BadFiles.Count > 0) {
            sb.Append("Corrupt files: ");
            sb.AppendLine(results.BadFiles.FormatStringCollection());
        }
        
        if (results.MissingFiles > 0) {
            sb.Append("Missing ");
            sb.Append(results.MissingFiles);
            sb.AppendLine(" files");
        }
        
        return sb.ToString();
    }

    private static string FormatStringCollection(this IList<string> strings)
    {
        return $"'{string.Join("', '", strings)}' ({strings.Count})";
    }
}