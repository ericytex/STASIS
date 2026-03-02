using Microsoft.AspNetCore.Identity;

namespace STASIS.Models;

public class ShipmentBatch
{
    public int BatchID { get; set; }
    public DateTime ImportDate { get; set; }
    public string? ImportedByUserId { get; set; }
    public IdentityUser? ImportedByUser { get; set; }
    public string? RequestorName { get; set; }
    public string? RequestorEmail { get; set; }
    public int TotalRequested { get; set; }
    public int TotalAvailable { get; set; }
    public int TotalNotFound { get; set; }
    public int TotalPreviouslyShipped { get; set; }
    public int TotalDiscarded { get; set; }
    public int TotalNotYetReceived { get; set; }
    public int? ApprovalID { get; set; }
    public Approval? Approval { get; set; }
    public string Status { get; set; } = "Pending Approval";
    public ICollection<ShipmentRequest> ShipmentRequests { get; set; } = new List<ShipmentRequest>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
