using System.Globalization;
using Microsoft.EntityFrameworkCore;
using STASIS.Data;
using STASIS.Models;

namespace STASIS.Services;

public class ShipmentService : IShipmentService
{
    private readonly StasisDbContext _context;
    private readonly IAuditService _auditService;

    public ShipmentService(StasisDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ShipmentBatch> ImportBatchFromCsvAsync(Stream csvStream, string userId,
        string? requestorName, string? requestorEmail)
    {
        var lines = new List<string>();
        using (var reader = new StreamReader(csvStream))
        {
            while (await reader.ReadLineAsync() is { } line)
                lines.Add(line);
        }

        // Create batch
        var batch = new ShipmentBatch
        {
            ImportDate = DateTime.UtcNow,
            ImportedByUserId = userId,
            RequestorName = requestorName,
            RequestorEmail = requestorEmail,
            Status = "Pending Approval"
        };
        _context.ShipmentBatches.Add(batch);
        await _context.SaveChangesAsync();

        // Create approval record
        var approval = new Approval
        {
            ApprovalType = "Shipment",
            RequestedByUserId = userId,
            RequestedDate = DateTime.UtcNow,
            OverallStatus = "Pending"
        };
        _context.Approvals.Add(approval);
        await _context.SaveChangesAsync();
        batch.ApprovalID = approval.ApprovalID;

        // Parse CSV (skip header if present)
        int startIndex = 0;
        if (lines.Count > 0 && lines[0].ToLowerInvariant().Contains("barcode"))
            startIndex = 1;

        var sampleTypes = await _context.SampleTypes.ToListAsync();
        int totalRequested = 0, totalAvailable = 0, totalNotFound = 0,
            totalPreviouslyShipped = 0, totalDiscarded = 0, totalNotYetReceived = 0;

        for (int i = startIndex; i < lines.Count; i++)
        {
            var parts = ParseCsvLine(lines[i]);
            if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0])) continue;

            var barcode = parts[0].Trim();
            var sampleTypeName = parts.Length > 1 ? parts[1].Trim() : null;
            var reqName = parts.Length > 2 ? parts[2].Trim() : requestorName;
            var reqDateStr = parts.Length > 3 ? parts[3].Trim() : null;

            totalRequested++;

            // Resolve sample type
            int? sampleTypeId = null;
            if (!string.IsNullOrEmpty(sampleTypeName))
            {
                var st = sampleTypes.FirstOrDefault(s =>
                    s.TypeName.Equals(sampleTypeName, StringComparison.OrdinalIgnoreCase));
                sampleTypeId = st?.SampleTypeID;
            }

            DateTime? reqDate = null;
            if (!string.IsNullOrEmpty(reqDateStr) &&
                DateTime.TryParse(reqDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                reqDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

            // Try to match specimen
            var specimen = await _context.Specimens
                .FirstOrDefaultAsync(s => s.BarcodeID == barcode);

            string status;
            int? matchedId = null;

            if (specimen == null)
            {
                status = "Not Found";
                totalNotFound++;
            }
            else if (specimen.Status == "Shipped")
            {
                status = "Previously Shipped";
                matchedId = specimen.SpecimenID;
                totalPreviouslyShipped++;
            }
            else if (specimen.Status == "Discarded")
            {
                status = "Discarded";
                matchedId = specimen.SpecimenID;
                totalDiscarded++;
            }
            else if (specimen.Status == "Not Yet Received")
            {
                status = "Not Yet Received";
                matchedId = specimen.SpecimenID;
                totalNotYetReceived++;
            }
            else
            {
                status = "Pending";
                matchedId = specimen.SpecimenID;
                totalAvailable++;
            }

            var request = new ShipmentRequest
            {
                BatchID = batch.BatchID,
                RequestedBarcode = barcode,
                RequestedSampleTypeID = sampleTypeId,
                RequestorName = reqName,
                RequestDate = reqDate ?? DateTime.UtcNow,
                MatchedSpecimenID = matchedId,
                Status = status
            };
            _context.ShipmentRequests.Add(request);
        }

        batch.TotalRequested = totalRequested;
        batch.TotalAvailable = totalAvailable;
        batch.TotalNotFound = totalNotFound;
        batch.TotalPreviouslyShipped = totalPreviouslyShipped;
        batch.TotalDiscarded = totalDiscarded;
        batch.TotalNotYetReceived = totalNotYetReceived;

        await _context.SaveChangesAsync();

        await _auditService.LogChangeAsync("tbl_ShipmentBatches", batch.BatchID.ToString(),
            "Imported", null,
            $"{totalRequested} requested, {totalAvailable} available", userId);

        return batch;
    }

    public async Task<List<ShipmentBatch>> GetAllBatchesAsync()
    {
        return await _context.ShipmentBatches
            .Include(b => b.ImportedByUser)
            .Include(b => b.Approval)
            .OrderByDescending(b => b.ImportDate)
            .ToListAsync();
    }

    public async Task<ShipmentBatch?> GetBatchByIdAsync(int batchId)
    {
        return await _context.ShipmentBatches
            .Include(b => b.ImportedByUser)
            .Include(b => b.Approval)
            .Include(b => b.ShipmentRequests)
                .ThenInclude(r => r.MatchedSpecimen)
                    .ThenInclude(s => s!.SampleType)
            .Include(b => b.ShipmentRequests)
                .ThenInclude(r => r.RequestedSampleType)
            .Include(b => b.Shipments)
            .FirstOrDefaultAsync(b => b.BatchID == batchId);
    }

    public async Task<List<Shipment>> GetShipmentsForBatchAsync(int batchId)
    {
        return await _context.Shipments
            .Where(s => s.BatchID == batchId)
            .Include(s => s.ShipmentContents)
                .ThenInclude(sc => sc.Specimen)
            .Include(s => s.ShippedByUser)
            .OrderByDescending(s => s.ShipmentDate)
            .ToListAsync();
    }

    public async Task<Shipment?> GetShipmentByIdAsync(int shipmentId)
    {
        return await _context.Shipments
            .Include(s => s.Batch)
            .Include(s => s.ShippedByUser)
            .Include(s => s.ShipmentContents)
                .ThenInclude(sc => sc.Specimen)
                    .ThenInclude(sp => sp!.SampleType)
            .FirstOrDefaultAsync(s => s.ShipmentID == shipmentId);
    }

    public async Task ApproveBatchAsync(int batchId, string approverUserId, string level, string status, string? comments)
    {
        var batch = await _context.ShipmentBatches
            .Include(b => b.Approval)
            .FirstOrDefaultAsync(b => b.BatchID == batchId);
        if (batch?.Approval == null) return;

        var approval = batch.Approval;
        var now = DateTime.UtcNow;

        switch (level.ToLowerInvariant())
        {
            case "ed":
                approval.EDApproverUserId = approverUserId;
                approval.EDApprovalStatus = status;
                approval.EDApprovalDate = now;
                approval.EDComments = comments;
                break;
            case "regulatory":
                approval.RegulatoryApproverUserId = approverUserId;
                approval.RegulatoryApprovalStatus = status;
                approval.RegulatoryApprovalDate = now;
                approval.RegulatoryComments = comments;
                break;
            case "pi":
                approval.PIApproverUserId = approverUserId;
                approval.PIApprovalStatus = status;
                approval.PIApprovalDate = now;
                approval.PIComments = comments;
                break;
        }

        // Check if any level was rejected
        if (approval.EDApprovalStatus == "Rejected" ||
            approval.RegulatoryApprovalStatus == "Rejected" ||
            approval.PIApprovalStatus == "Rejected")
        {
            approval.OverallStatus = "Rejected";
            batch.Status = "Rejected";
        }
        // Check if all applicable levels are approved
        else if (approval.EDApprovalStatus == "Approved" &&
                 approval.RegulatoryApprovalStatus == "Approved")
        {
            approval.OverallStatus = "Approved";
            batch.Status = "Approved";
        }

        await _context.SaveChangesAsync();

        await _auditService.LogChangeAsync("tbl_Approvals", approval.ApprovalID.ToString(),
            $"{level}Approval", null, status, approverUserId);
    }

    public async Task<Shipment> ShipBatchAsync(int batchId, string courier, string? trackingNumber,
        string? destination, string userId)
    {
        var batch = await _context.ShipmentBatches
            .Include(b => b.ShipmentRequests)
                .ThenInclude(r => r.MatchedSpecimen)
            .FirstOrDefaultAsync(b => b.BatchID == batchId);

        if (batch == null) throw new InvalidOperationException("Batch not found.");

        var shipment = new Shipment
        {
            BatchID = batchId,
            ShipmentDate = DateTime.UtcNow,
            Courier = courier,
            TrackingNumber = trackingNumber,
            DestinationAddress = destination,
            ShippedByUserId = userId
        };
        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        // Process pending requests with matched specimens
        foreach (var request in batch.ShipmentRequests
            .Where(r => r.Status == "Pending" && r.MatchedSpecimenID != null))
        {
            var specimen = request.MatchedSpecimen!;
            specimen.Status = "Shipped";
            request.Status = "Shipped";

            var content = new ShipmentContent
            {
                ShipmentID = shipment.ShipmentID,
                SpecimenID = specimen.SpecimenID
            };
            _context.ShipmentContents.Add(content);
        }

        batch.Status = "Shipped";
        await _context.SaveChangesAsync();

        await _auditService.LogChangeAsync("tbl_Shipments", shipment.ShipmentID.ToString(),
            "Created", null, $"Batch {batchId}, Courier: {courier}", userId);

        return shipment;
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
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
