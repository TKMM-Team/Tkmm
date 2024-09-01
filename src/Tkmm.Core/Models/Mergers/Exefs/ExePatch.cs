using Revrs;
using Revrs.Extensions;
using System.Globalization;
using TotkCommon;

namespace Tkmm.Core.Models.Mergers.Exefs;

public class ExePatch
{
    private const uint IPS32_MAGIC = 0x33535049; // The first 4 chars of magic (IPS3)
    private const byte IPS32_MAGIC_END = 0x32; // The last byte of magic (2)
    private const uint EOF_MARK = 0x45454F46;
    private const uint NSO_HEADER_LENGTH = 0x100;

    private readonly string _expectedPchtxtHeader = $"@nsobid-{Totk.Config.NSOBID}";
    private readonly Dictionary<uint, uint> _values = [];

    private enum State
    {
        None,
        Enabled,
    }

    public void Write(string output)
    {
        string outputFolder = Path.Combine(output, TotkConfig.EXEFS);
        Directory.CreateDirectory(outputFolder);

        string outputPath = Path.Combine(outputFolder, $"{Totk.Config.NSOBID.ToUpper()}.ips");
        using FileStream fs = File.Create(outputPath);

        fs.Write(IPS32_MAGIC);
        fs.Write(IPS32_MAGIC_END);

        foreach (var (address, value) in _values) {
            fs.Write(address + NSO_HEADER_LENGTH, Endianness.Big);
            fs.Write<short>(sizeof(uint), Endianness.Big); // value size
            fs.Write(value, Endianness.Big);
        }

        fs.Write(EOF_MARK, Endianness.Big);
    }

    public void AppendIps(string file)
    {
        string ipsId = Path.GetFileNameWithoutExtension(file);
        if (!string.Equals(ipsId, Totk.Config.NSOBID, StringComparison.InvariantCultureIgnoreCase)) {
            AppLog.Log($"Unexpected IPS version, expected '{Totk.Config.NSOBID}' but found '{ipsId}'. Skipping '{file}'.", LogLevel.Warning);
            return;
        }

        using FileStream fs = File.OpenRead(file);
        if (fs.Read<uint>() is not IPS32_MAGIC || fs.ReadByte() is not IPS32_MAGIC_END) {
            AppLog.Log($"Invalid IPS32 magic (PATCH is not supported). Skipping '{file}'", LogLevel.Warning);
            return;
        }

        uint address = fs.Read<uint>(Endianness.Big);
        while (address is not EOF_MARK) {
            int valueSize = fs.Read<short>(Endianness.Big);
            if (valueSize is not 4) {
                AppLog.Log($"Unexpected value size, expected '4' but found '{valueSize}'. Address '{address}' skipped.", LogLevel.Warning);
                goto NextAddress;
            }

            uint value = fs.Read<uint>(Endianness.Big);
            _values[address - NSO_HEADER_LENGTH] = value;

        NextAddress:
            address = fs.Read<uint>(Endianness.Big);
        }
    }

    private const string ENABLED_KEYWORD = "@enabled";
    private const char COMMENT_CHAR = '@';
    private const string STOP_KEYWORD = "@stop";

    public void AppendPchtxt(string file)
    {
        State state = State.None;

        using FileStream fs = File.OpenRead(file);
        using StreamReader reader = new(fs);

        int lineNumber = 0;

        while (reader.ReadLine() is string line) {
            lineNumber++;

            if (lineNumber == 1) {
                if (!line.StartsWith(_expectedPchtxtHeader, StringComparison.InvariantCultureIgnoreCase)) {
                    AppLog.Log($"Unexpected pchtxt version, expected '{_expectedPchtxtHeader}' but found '{line}'. Skipping '{file}'.", LogLevel.Warning);
                    goto Skip;
                }

                continue;
            }

            if (state is State.Enabled) {
                if (line.StartsWith(STOP_KEYWORD)) {
                    state = State.None;
                    continue;
                }

                if (line.Length > 0 && line[0] == COMMENT_CHAR) {
                    continue;
                }

                ReadOnlySpan<char> chars = line.AsSpan();
                int addressEndIndex = GetValueEndIndex(chars, 0);
                if (!uint.TryParse(chars[0..addressEndIndex], NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out uint address)) {
                    AppLog.Log($"Could not parse entry address '{chars[0..8]}' skipping '{line}'.", LogLevel.Warning);
                    continue;
                }

                int valueEndIndex = GetValueEndIndex(chars, ++addressEndIndex);
                if (!uint.TryParse(chars[addressEndIndex..valueEndIndex], NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out uint value)) {
                    AppLog.Log($"Could not parse entry value '{chars[9..17]}' skipping '{line}'.", LogLevel.Warning);
                    continue;
                }

                _values[address] = value;
                continue;
            }

            if (line.StartsWith(ENABLED_KEYWORD)) {
                state = State.Enabled;
            }
        }

    Skip:
        ;
    }

    private static int GetValueEndIndex(ReadOnlySpan<char> chars, int startIndex)
    {
        int endIndex = startIndex;
        while (endIndex < chars.Length && chars[endIndex] is (>= 'A' and <= 'F') or (>= 'a' and <= 'f') or (>= '0' and <= '9')) {
            endIndex++;
        }

        return endIndex;
    }
}
