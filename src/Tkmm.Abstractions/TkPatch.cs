using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Revrs;
using Revrs.Extensions;

namespace Tkmm.Abstractions;

public class TkPatch(string id) : Dictionary<uint, uint>
{
    private const uint IPS32_MAGIC_PREFIX = 0x33535049;
    private const byte IPS32_MAGIC_SUFFIX = 0x32;
    private const uint EOF_MARK = 0x45454F46;
    private const uint NSO_HEADER_LENGTH = 0x100;

    public string Id { get; } = id;
    
    public static async ValueTask<TkPatch?> FromIps(Stream stream, string nsobid)
    {
        // TODO: This should use ReadAsync for async-only streams
        
        if (stream.Read<uint>() is not IPS32_MAGIC_PREFIX || stream.ReadByte() is not IPS32_MAGIC_SUFFIX) {
            return null;
        }

        TkPatch patch = new(nsobid);

        uint address = stream.Read<uint>(Endianness.Big);
        while (address is not EOF_MARK) {
            int valueSize = stream.Read<short>(Endianness.Big);
            if (valueSize is not 4) {
                goto NextAddress;
            }

            uint value = stream.Read<uint>(Endianness.Big);
            patch[address - NSO_HEADER_LENGTH] = value;

        NextAddress:
            address = stream.Read<uint>(Endianness.Big);
        }

        return patch;
    }
    
    private const string NSOBID_KEYWORD = "@nsobid";
    private const string ENABLED_KEYWORD = "@enabled";
    private const char COMMENT_CHAR = '@';
    private const string STOP_KEYWORD = "@stop";

    public static async ValueTask<TkPatch?> FromPchtxt(Stream stream, CancellationToken ct = default)
    {
        using StreamReader reader = new(stream);

        if (await reader.ReadLineAsync(ct) is not string firstLine) {
            return null;
        }

        if (!TryGetNsobid(firstLine, out string? nsobid)) {
            return null;
        }
        
        TkPatch patch = new(nsobid);
        
        var state = State.None;
        while (await reader.ReadLineAsync(ct) is string line) {
            if (state is State.Enabled) {
                if (line.StartsWith(STOP_KEYWORD)) {
                    state = State.None;
                    continue;
                }

                if (line.Length > 0 && line[0] == COMMENT_CHAR) {
                    continue;
                }

                if (GetAddressAndValue(line) is not { } values) {
                    continue;
                }

                patch[values.Address] = values.Value;
                continue;
            }

            if (line.StartsWith(ENABLED_KEYWORD)) {
                state = State.Enabled;
            }
        }

        return patch;
    }

    private static bool TryGetNsobid(ReadOnlySpan<char> line, [MaybeNullWhen(false)] out string nsobid)
    {
        if (line is not { Length: > 7 } || line[..7] is not NSOBID_KEYWORD) {
            nsobid = null;
            return false;
        }

        nsobid = line[8..].ToString();
        return true;
    }

    private static (uint Address, uint Value)? GetAddressAndValue(ReadOnlySpan<char> line)
    {
        int addressEndIndex = GetValueEndIndex(line, 0);
        if (!uint.TryParse(line[..addressEndIndex], NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out uint address)) {
            return null;
        }

        int valueEndIndex = GetValueEndIndex(line, ++addressEndIndex);
        if (!uint.TryParse(line[addressEndIndex..valueEndIndex], NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out uint value)) {
            return null;
        }
        
        return (address, value);
    }
    
    private static int GetValueEndIndex(ReadOnlySpan<char> chars, int startIndex)
    {
        int endIndex = startIndex;
        while (endIndex < chars.Length && chars[endIndex] is >= 'A' and <= 'F' or >= 'a' and <= 'f' or >= '0' and <= '9') {
            endIndex++;
        }

        return endIndex;
    }
}

file enum State
{
    None,
    Enabled,
}