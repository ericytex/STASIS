namespace STASIS.Models;

public class ShipmentContent
{
    public int ShipmentContentID { get; set; }
    public int ShipmentID { get; set; }
    public Shipment? Shipment { get; set; }
    public int SpecimenID { get; set; }
    public Specimen? Specimen { get; set; }
    public string? ConditionAtShipment { get; set; }
    public string? ShippingBoxPosition { get; set; }
    public int? SpotsUsed { get; set; }
}
