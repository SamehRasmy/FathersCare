using FathersCare.Domain.Common;

namespace FathersCare.Domain.Residents;

public sealed class ResidentDocument : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Resident? Resident { get; set; }
    public ResidentDocumentType DocumentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? UploadedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsConfidential { get; set; }
}
