using System.Globalization;
using Microsoft.EntityFrameworkCore;
using STASIS.Data;
using STASIS.Models;

namespace STASIS.Services;

public class SampleService : ISampleService
{
    private readonly StasisDbContext _context;

    public SampleService(StasisDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Specimen> Specimens, int TotalCount)> GetSpecimensAsync(string? searchString, int? studyId, int? sampleTypeId, int pageIndex, int pageSize)
    {
        var query = _context.Specimens
            .Include(s => s.Study)
            .Include(s => s.SampleType)
            .Include(s => s.Box)
                .ThenInclude(b => b!.Rack)
                .ThenInclude(r => r!.Freezer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(s => s.BarcodeID.Contains(searchString));
        }

        if (studyId.HasValue)
        {
            query = query.Where(s => s.StudyID == studyId.Value);
        }

        if (sampleTypeId.HasValue)
        {
            query = query.Where(s => s.SampleTypeID == sampleTypeId.Value);
        }

        var totalCount = await query.CountAsync();

        var specimens = await query.Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

        return (specimens, totalCount);
    }

    public async Task<List<Study>> GetAllStudies()
    {
        return await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
    }

    public async Task<List<SampleType>> GetAllSampleTypes()
    {
        return await _context.SampleTypes.OrderBy(st => st.TypeName).ToListAsync();
    }

    public async Task<Specimen?> GetSpecimenByBarcode(string barcode)
    {
        return await _context.Specimens
            .Include(s => s.Study)
            .Include(s => s.SampleType)
            .Include(s => s.Box)
            .ThenInclude(b => b!.Rack)
            .ThenInclude(r => r!.Freezer)
            .FirstOrDefaultAsync(s => s.BarcodeID == barcode);
    }

    public async Task<bool> IsBarcodeTaken(string barcode)
    {
        return await _context.Specimens.AnyAsync(s => s.BarcodeID == barcode);
    }

    public async Task<List<(int Row, int Col)>> GetOccupiedPositions(int boxId)
    {
        return await _context.Specimens
            .Where(s => s.BoxID == boxId && s.PositionRow != null && s.PositionCol != null)
            .Select(s => new ValueTuple<int, int>(s.PositionRow!.Value, s.PositionCol!.Value))
            .ToListAsync();
    }

    public async Task AddSpecimen(Specimen specimen)
    {
        _context.Specimens.Add(specimen);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSpecimen(Specimen specimen)
    {
        _context.Entry(specimen).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSpecimen(int id)
    {
        var specimen = await _context.Specimens.FindAsync(id);
        if (specimen != null)
        {
            _context.Specimens.Remove(specimen);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ImportResult> ImportSpecimensFromCsv(Stream csvStream)
    {
        var result = new ImportResult();
        var lines = new List<string>();

        using (var reader = new StreamReader(csvStream))
        {
            while (await reader.ReadLineAsync() is { } line)
                lines.Add(line);
        }

        if (lines.Count == 0)
            return result;

        // Skip header row
        var headerLine = lines[0].ToLowerInvariant();
        var startIndex = headerLine.Contains("barcode") ? 1 : 0;

        result.TotalRows = lines.Count - startIndex;

        // Pre-load lookup data
        var studies = await _context.Studies.ToListAsync();
        var sampleTypes = await _context.SampleTypes.ToListAsync();
        var boxes = await _context.Boxes.ToListAsync();
        var existingBarcodes = await _context.Specimens
            .Select(s => s.BarcodeID)
            .ToListAsync();
        var existingBarcodeSet = new HashSet<string>(existingBarcodes, StringComparer.OrdinalIgnoreCase);

        // Track barcodes within this import to detect intra-file duplicates
        var importBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Track positions claimed within this import
        var claimedPositions = new HashSet<string>();

        for (int i = startIndex; i < lines.Count; i++)
        {
            var row = ParseCsvLine(lines[i]);
            var importRow = new ImportRow { LineNumber = i + 1 };

            if (row.Length < 1 || string.IsNullOrWhiteSpace(row[0]))
            {
                importRow.Error = "BarcodeID is required.";
                result.ErrorRows.Add(importRow);
                continue;
            }

            importRow.BarcodeID = row[0].Trim();
            importRow.LegacyID = row.Length > 1 ? row[1].Trim() : null;
            importRow.StudyCode = row.Length > 2 ? row[2].Trim() : null;
            importRow.SampleType = row.Length > 3 ? row[3].Trim() : null;
            importRow.CollectionDate = row.Length > 4 ? row[4].Trim() : null;
            importRow.BoxLabel = row.Length > 5 ? row[5].Trim() : null;
            importRow.PositionRow = row.Length > 6 ? row[6].Trim() : null;
            importRow.PositionCol = row.Length > 7 ? row[7].Trim() : null;

            // Validate barcode uniqueness
            if (existingBarcodeSet.Contains(importRow.BarcodeID))
            {
                importRow.Error = $"Barcode '{importRow.BarcodeID}' already exists in the database.";
                result.ErrorRows.Add(importRow);
                continue;
            }

            if (!importBarcodes.Add(importRow.BarcodeID))
            {
                importRow.Error = $"Duplicate barcode '{importRow.BarcodeID}' within this import file.";
                result.ErrorRows.Add(importRow);
                continue;
            }

            // Resolve study
            int? studyId = null;
            if (!string.IsNullOrEmpty(importRow.StudyCode))
            {
                var study = studies.FirstOrDefault(s =>
                    s.StudyCode.Equals(importRow.StudyCode, StringComparison.OrdinalIgnoreCase));
                if (study == null)
                {
                    importRow.Error = $"Study code '{importRow.StudyCode}' not found.";
                    result.ErrorRows.Add(importRow);
                    continue;
                }
                studyId = study.StudyID;
            }

            // Resolve sample type
            int? sampleTypeId = null;
            SampleType? resolvedSampleType = null;
            if (!string.IsNullOrEmpty(importRow.SampleType))
            {
                resolvedSampleType = sampleTypes.FirstOrDefault(st =>
                    st.TypeName.Equals(importRow.SampleType, StringComparison.OrdinalIgnoreCase));
                if (resolvedSampleType == null)
                {
                    importRow.Error = $"Sample type '{importRow.SampleType}' not found.";
                    result.ErrorRows.Add(importRow);
                    continue;
                }
                sampleTypeId = resolvedSampleType.SampleTypeID;
            }

            // Parse collection date
            DateTime? collectionDate = null;
            if (!string.IsNullOrEmpty(importRow.CollectionDate))
            {
                if (!DateTime.TryParse(importRow.CollectionDate, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsed))
                {
                    importRow.Error = $"Invalid date format: '{importRow.CollectionDate}'.";
                    result.ErrorRows.Add(importRow);
                    continue;
                }
                collectionDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            // Resolve box
            int? boxId = null;
            if (!string.IsNullOrEmpty(importRow.BoxLabel))
            {
                var box = boxes.FirstOrDefault(b =>
                    b.BoxLabel.Equals(importRow.BoxLabel, StringComparison.OrdinalIgnoreCase));
                if (box == null)
                {
                    importRow.Error = $"Box label '{importRow.BoxLabel}' not found.";
                    result.ErrorRows.Add(importRow);
                    continue;
                }
                boxId = box.BoxID;
            }

            // Parse position
            int? posRow = null, posCol = null;
            if (!string.IsNullOrEmpty(importRow.PositionRow) && !string.IsNullOrEmpty(importRow.PositionCol))
            {
                if (!int.TryParse(importRow.PositionRow, out var pr) ||
                    !int.TryParse(importRow.PositionCol, out var pc))
                {
                    importRow.Error = "PositionRow and PositionCol must be integers.";
                    result.ErrorRows.Add(importRow);
                    continue;
                }
                posRow = pr;
                posCol = pc;

                // Check position not already claimed in this import
                if (boxId.HasValue)
                {
                    var posKey = $"{boxId.Value}-{pr}-{pc}";
                    if (!claimedPositions.Add(posKey))
                    {
                        importRow.Error = $"Position ({pr},{pc}) in box '{importRow.BoxLabel}' is used by another row in this file.";
                        result.ErrorRows.Add(importRow);
                        continue;
                    }

                    // Check against DB
                    var occupied = await _context.Specimens.AnyAsync(s =>
                        s.BoxID == boxId.Value && s.PositionRow == pr && s.PositionCol == pc);
                    if (occupied)
                    {
                        importRow.Error = $"Position ({pr},{pc}) in box '{importRow.BoxLabel}' is already occupied.";
                        result.ErrorRows.Add(importRow);
                        continue;
                    }
                }
            }

            // Set filter paper defaults
            int? remainingSpots = null;
            if (resolvedSampleType != null &&
                resolvedSampleType.TypeName.Equals("Filter Paper", StringComparison.OrdinalIgnoreCase))
            {
                remainingSpots = 4;
            }

            var specimen = new Specimen
            {
                BarcodeID = importRow.BarcodeID,
                LegacyID = string.IsNullOrEmpty(importRow.LegacyID) ? null : importRow.LegacyID,
                StudyID = studyId,
                SampleTypeID = sampleTypeId,
                CollectionDate = collectionDate,
                BoxID = boxId,
                PositionRow = posRow,
                PositionCol = posCol,
                RemainingSpots = remainingSpots,
                Status = "In-Stock"
            };

            importRow.Specimen = specimen;
            result.SuccessRows.Add(importRow);
        }

        return result;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
