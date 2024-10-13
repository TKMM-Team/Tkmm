namespace Tkmm.Abstractions;

public interface ITkResourceSizeTable
{
    void CalculateAndAdd(string canonicalResourceName, uint resourceSize, Span<byte> data = default);

    ValueTask Write(Stream output, CancellationToken ct = default);
}