namespace STASIS.Models;

public class Study
{
    public int StudyID { get; set; }
    public string StudyCode { get; set; } = string.Empty;
    public string? StudyName { get; set; }
    public string? PrincipalInvestigator { get; set; }
    public ICollection<Specimen> Specimens { get; set; } = new List<Specimen>();
}
