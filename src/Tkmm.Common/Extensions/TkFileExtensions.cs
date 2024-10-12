using System.Runtime.CompilerServices;
using Tkmm.Abstractions;

namespace Tkmm.Common.Extensions;

/// <summary>
/// Provides extension methods for TotK file paths and the <see cref="TkFileInfo"/> struct.
/// </summary>
public static class TkFileExtensions
{
    public static TkFileInfo GetTkFileInfo(this string filePath, string romfs)
    {
        ReadOnlySpan<char> canonical = GetCanonical(filePath, romfs, out TkFileAttributes attributes);
        ReadOnlySpan<char> extension = Path.GetExtension(canonical);
        
        return new TkFileInfo(
            filePath,
            romfs,
            canonical,
            extension,
            attributes
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this string fileRelativeToRomfs)
    {
        return GetCanonical(fileRelativeToRomfs.AsSpan(), [], out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this ReadOnlySpan<char> fileRelativeToRomfs)
    {
        return GetCanonical(fileRelativeToRomfs, [], out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this string fileRelativeToRomfs, out TkFileAttributes attributes)
    {
        return GetCanonical(fileRelativeToRomfs, [], out attributes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this ReadOnlySpan<char> fileRelativeToRomfs, out TkFileAttributes attributes)
    {
        return GetCanonical(fileRelativeToRomfs, [], out attributes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this string file, ReadOnlySpan<char> romfs)
    {
        return GetCanonical(file.AsSpan(), romfs, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this string file, ReadOnlySpan<char> romfs, out TkFileAttributes attributes)
    {
        return GetCanonical(file.AsSpan(), romfs, out attributes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetCanonical(this ReadOnlySpan<char> file, ReadOnlySpan<char> romfs)
    {
        return GetCanonical(file, romfs, out _);
    }

    public static unsafe ReadOnlySpan<char> GetCanonical(this ReadOnlySpan<char> file, ReadOnlySpan<char> romfs, out TkFileAttributes attributes)
    {
        if (file.Length < romfs.Length)
        {
            throw new ArgumentException(
                $"The provided {nameof(romfs)} path is longer than the input {nameof(file)}.", nameof(romfs)
            );
        }

        attributes = 0;

        int size = file.Length - romfs.Length - file[^3..] switch {
            ".zs" => (int)(attributes |= TkFileAttributes.HasZsExtension) + 2,
            ".mc" => (int)(attributes |= TkFileAttributes.HasMcExtension) + 1,
            _ => 0
        };

        // Make a copy to avoid
        // mutating the input string
        string result = file[romfs.Length..(romfs.Length + size)].ToString();

        Span<char> canonical;

        fixed (char* ptr = result)
        {
            canonical = new Span<char>(ptr, size);
        }

        int state = 0;
        for (int i = 0; i < size; i++)
        {
            ref char @char = ref canonical[i];

            state = (@char, size - i) switch {
                ('.', > 8) => canonical[i..(i + 8)] switch {
                    ".Product" => ((int)(attributes |= TkFileAttributes.IsProductFile) * (size -= 4) * (i += 8)) + 1,
                    _ => state
                },
                _ => state
            };

            @char = state switch {
                0 => @char,
                _ => @char = canonical[i + 4]
            };

            @char = @char switch {
                '\\' => '/',
                _ => @char
            };
        }

        return canonical[0] switch {
            '/' => canonical[1..size],
            _ => canonical[..size]
        };
    }
}