using Microsoft.AspNetCore.Identity;

namespace STASIS.Models;

public class Shipment
{
    public int ShipmentID { get; set; }
    public int? BatchID { get; set; }
    public ShipmentBatch? Batch { get; set; }
    public DateTime ShipmentDate { get; set; }
    public string? Courier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? DestinationAddress { get; set; }
    public string? ShippedByUserId { get; set; }
    public IdentityUser? ShippedByUser { get; set; }
    public bool IsEntireBox { get; set; }
    public int? ShippedBoxID { get; set; }
    public Box? ShippedBox { get; set; }
    public ICollection<ShipmentContent> ShipmentContents { get; set; } = new List<ShipmentContent>();
}
