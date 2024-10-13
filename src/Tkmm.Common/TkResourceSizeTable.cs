using Tkmm.Abstractions;

namespace Tkmm.Common;

public sealed class TkResourceSizeTable : ITkResourceSizeTable
{
    public void CalculateAndAdd(string canonicalResourceName, uint resourceSize, Span<byte> data = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(Stream output, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}