using System.ComponentModel.DataAnnotations;

namespace STASIS.Models;

public class ShipmentRequest
{
    [Key]
    public int RequestID { get; set; }
    public int? BatchID { get; set; }
    public ShipmentBatch? Batch { get; set; }
    public string RequestedBarcode { get; set; } = string.Empty;
    public int? RequestedSampleTypeID { get; set; }
    public SampleType? RequestedSampleType { get; set; }
    public string? RequestorName { get; set; }
    public DateTime? RequestDate { get; set; }
    public int? MatchedSpecimenID { get; set; }
    public Specimen? MatchedSpecimen { get; set; }
    public string Status { get; set; } = "Pending";
}
