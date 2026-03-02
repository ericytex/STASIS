using Microsoft.AspNetCore.Identity;

namespace STASIS.Models;

public class Approval
{
    public int ApprovalID { get; set; }
    public string ApprovalType { get; set; } = "Shipment";
    public string? RequestedByUserId { get; set; }
    public IdentityUser? RequestedByUser { get; set; }
    public DateTime RequestedDate { get; set; }
    public string? EDApproverUserId { get; set; }
    public IdentityUser? EDApproverUser { get; set; }
    public DateTime? EDApprovalDate { get; set; }
    public string? EDApprovalStatus { get; set; }
    public string? EDComments { get; set; }
    public string? RegulatoryApproverUserId { get; set; }
    public IdentityUser? RegulatoryApproverUser { get; set; }
    public DateTime? RegulatoryApprovalDate { get; set; }
    public string? RegulatoryApprovalStatus { get; set; }
    public string? RegulatoryComments { get; set; }
    public string? PIApproverUserId { get; set; }
    public IdentityUser? PIApproverUser { get; set; }
    public DateTime? PIApprovalDate { get; set; }
    public string? PIApprovalStatus { get; set; }
    public string? PIComments { get; set; }
    public string OverallStatus { get; set; } = "Pending";
    public ICollection<Specimen> DiscardedSpecimens { get; set; } = new List<Specimen>();
    public ICollection<ShipmentBatch> ShipmentBatches { get; set; } = new List<ShipmentBatch>();
}
