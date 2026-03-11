using STASIS.Models;

namespace STASIS.Services;

public interface ISampleService
{
    Task<(List<Specimen> Specimens, int TotalCount)> GetSpecimensAsync(string? searchString, int? studyId, int? sampleTypeId, int pageIndex, int pageSize);
    Task<List<Study>> GetAllStudies();
    Task<List<SampleType>> GetAllSampleTypes();
    Task<Specimen?> GetSpecimenByBarcode(string barcode);
    Task<bool> IsBarcodeTaken(string barcode);
    Task<List<(int Row, int Col)>> GetOccupiedPositions(int boxId);
    Task AddSpecimen(Specimen specimen);
    Task UpdateSpecimen(Specimen specimen);
    Task DeleteSpecimen(int id);
    Task<ImportResult> ImportSpecimensFromCsv(Stream csvStream);
}

public class ImportResult
{
    public List<ImportRow> SuccessRows { get; set; } = new();
    public List<ImportRow> ErrorRows { get; set; } = new();
    public int TotalRows { get; set; }
}

public class ImportRow
{
    public int LineNumber { get; set; }
    public string BarcodeID { get; set; } = string.Empty;
    public string? LegacyID { get; set; }
    public string? StudyCode { get; set; }
    public string? SampleType { get; set; }
    public string? CollectionDate { get; set; }
    public string? BoxLabel { get; set; }
    public string? PositionRow { get; set; }
    public string? PositionCol { get; set; }
    public string? Error { get; set; }
    public Specimen? Specimen { get; set; }
}
