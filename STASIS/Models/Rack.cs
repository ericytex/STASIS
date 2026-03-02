namespace STASIS.Models;

public class Rack
{
    public int RackID { get; set; }
    public string RackName { get; set; } = string.Empty;
    public int? FreezerID { get; set; }
    public Freezer? Freezer { get; set; }
    public ICollection<Box> Boxes { get; set; } = new List<Box>();
}
