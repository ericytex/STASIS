namespace STASIS.Models;

public class SampleType
{
    public int SampleTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsConsumable { get; set; }
    public int? MaxShippableUnits { get; set; }
    public int? LocalReserveUnits { get; set; }
    public ICollection<Specimen> Specimens { get; set; } = new List<Specimen>();
    public ICollection<ShipmentRequest> ShipmentRequests { get; set; } = new List<ShipmentRequest>();
}
