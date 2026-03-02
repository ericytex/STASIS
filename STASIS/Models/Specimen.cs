namespace STASIS.Models;

public class Specimen
{
    public int SpecimenID { get; set; }
    public string BarcodeID { get; set; } = string.Empty;
    public string? LegacyID { get; set; }
    public int? StudyID { get; set; }
    public Study? Study { get; set; }
    public int? SampleTypeID { get; set; }
    public SampleType? SampleType { get; set; }
    public DateTime? CollectionDate { get; set; }
    public int? BoxID { get; set; }
    public Box? Box { get; set; }
    public int? PositionRow { get; set; }
    public int? PositionCol { get; set; }
    public int? RemainingSpots { get; set; }
    public int SpotsShippedInternational { get; set; }
    public int SpotsReservedLocal { get; set; }
    public int? AliquotNumber { get; set; }
    public int? DiscardApprovalID { get; set; }
    public Approval? DiscardApproval { get; set; }
    public string Status { get; set; } = "In-Stock";
    public ICollection<ShipmentRequest> ShipmentRequests { get; set; } = new List<ShipmentRequest>();
    public ICollection<ShipmentContent> ShipmentContents { get; set; } = new List<ShipmentContent>();
    public ICollection<FilterPaperUsage> FilterPaperUsages { get; set; } = new List<FilterPaperUsage>();
}
