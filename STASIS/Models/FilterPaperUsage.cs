using Microsoft.AspNetCore.Identity;

namespace STASIS.Models;

public class FilterPaperUsage
{
    public int FilterPaperUsageID { get; set; }
    public int SpecimenID { get; set; }
    public Specimen? Specimen { get; set; }
    public DateTime UsageDate { get; set; }
    public int SpotsUsed { get; set; }
    public bool IsInternationalShipment { get; set; }
    public string? UsedByUserId { get; set; }
    public IdentityUser? UsedByUser { get; set; }
    public int? ShipmentContentID { get; set; }
    public ShipmentContent? ShipmentContent { get; set; }
    public string? Notes { get; set; }
}
