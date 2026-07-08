namespace FathersCare.Application.Abstractions;

public interface IReportService
{
    Task<byte[]> CreatePdfAsync(string reportName, object model, CancellationToken cancellationToken = default);
    Task<byte[]> CreateExcelAsync(string reportName, object model, CancellationToken cancellationToken = default);
}
