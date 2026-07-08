using FathersCare.Application.Abstractions;

namespace FathersCare.Infrastructure.Reports;

public sealed class CompositeReportService : IReportService
{
    public Task<byte[]> CreatePdfAsync(string reportName, object model, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }

    public Task<byte[]> CreateExcelAsync(string reportName, object model, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
