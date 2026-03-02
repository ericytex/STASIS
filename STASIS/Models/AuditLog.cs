using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STASIS.Models;

public class AuditLog
{
    [Key]
    public int AuditLogID { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int RecordID { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public string? ChangedByUserId { get; set; }
    
    [ForeignKey("ChangedByUserId")]
    public IdentityUser? User { get; set; }
    
    public DateTime Timestamp { get; set; }
}
