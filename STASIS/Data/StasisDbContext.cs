using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using STASIS.Models;

namespace STASIS.Data;

public class StasisDbContext : IdentityDbContext
{
    public StasisDbContext(DbContextOptions<StasisDbContext> options) : base(options)
    {
    }

    public DbSet<Study> Studies { get; set; }
    public DbSet<SampleType> SampleTypes { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Freezer> Freezers { get; set; }
    public DbSet<Rack> Racks { get; set; }
    public DbSet<Box> Boxes { get; set; }
    public DbSet<Specimen> Specimens { get; set; }
    public DbSet<Approval> Approvals { get; set; }
    public DbSet<ShipmentBatch> ShipmentBatches { get; set; }
    public DbSet<ShipmentRequest> ShipmentRequests { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentContent> ShipmentContents { get; set; }
    public DbSet<FilterPaperUsage> FilterPaperUsages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Study>().ToTable("tbl_Studies");
        modelBuilder.Entity<SampleType>().ToTable("tbl_SampleTypes");
        modelBuilder.Entity<UserProfile>().ToTable("tbl_UserProfiles");
        modelBuilder.Entity<Freezer>().ToTable("tbl_Freezers");
        modelBuilder.Entity<Rack>().ToTable("tbl_Racks");
        modelBuilder.Entity<Box>().ToTable("tbl_Boxes");
        modelBuilder.Entity<Specimen>().ToTable("tbl_Specimens");
        modelBuilder.Entity<Approval>().ToTable("tbl_Approvals");
        modelBuilder.Entity<ShipmentBatch>().ToTable("tbl_ShipmentBatches");
        modelBuilder.Entity<ShipmentRequest>().ToTable("tbl_ShipmentRequests");
        modelBuilder.Entity<Shipment>().ToTable("tbl_Shipments");
        modelBuilder.Entity<ShipmentContent>().ToTable("tbl_ShipmentContents");
        modelBuilder.Entity<FilterPaperUsage>().ToTable("tbl_FilterPaperUsages");
        modelBuilder.Entity<AuditLog>().ToTable("tbl_AuditLog");

        modelBuilder.Entity<UserProfile>()
            .HasIndex(p => p.AspNetUserId)
            .IsUnique();

        modelBuilder.Entity<Freezer>()
            .HasIndex(f => f.FreezerName)
            .IsUnique();

        modelBuilder.Entity<Box>()
            .HasIndex(b => b.BoxLabel)
            .IsUnique();

        modelBuilder.Entity<Box>()
            .Property(b => b.BoxCategory)
            .HasDefaultValue("Standard");

        modelBuilder.Entity<Box>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Boxes_BoxType",
                "\"BoxType\" IN ('81-slot', '100-slot', 'Filter Paper Binder')"));

        modelBuilder.Entity<Box>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Boxes_BoxCategory",
                "\"BoxCategory\" IN ('Standard', 'Temp', 'Trash', 'Shipping')"));

        modelBuilder.Entity<Study>()
            .HasIndex(s => s.StudyCode)
            .IsUnique();

        modelBuilder.Entity<SampleType>()
            .HasIndex(st => st.TypeName)
            .IsUnique();

        modelBuilder.Entity<Approval>()
            .Property(a => a.RequestedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Approval>()
            .Property(a => a.OverallStatus)
            .HasDefaultValue("Pending");

        modelBuilder.Entity<Approval>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Approvals_ApprovalType",
                "\"ApprovalType\" IN ('Shipment', 'Discard', 'SingleAliquotException')"));

        modelBuilder.Entity<Approval>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Approvals_EDApprovalStatus",
                "\"EDApprovalStatus\" IS NULL OR \"EDApprovalStatus\" IN ('Pending', 'Approved', 'Rejected')"));

        modelBuilder.Entity<Approval>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Approvals_RegulatoryApprovalStatus",
                "\"RegulatoryApprovalStatus\" IS NULL OR \"RegulatoryApprovalStatus\" IN ('Pending', 'Approved', 'Rejected')"));

        modelBuilder.Entity<Approval>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Approvals_PIApprovalStatus",
                "\"PIApprovalStatus\" IS NULL OR \"PIApprovalStatus\" IN ('Pending', 'Approved', 'Rejected')"));

        modelBuilder.Entity<Approval>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Approvals_OverallStatus",
                "\"OverallStatus\" IN ('Pending', 'Approved', 'Rejected')"));

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.RequestedByUser)
            .WithMany()
            .HasForeignKey(a => a.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.EDApproverUser)
            .WithMany()
            .HasForeignKey(a => a.EDApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.RegulatoryApproverUser)
            .WithMany()
            .HasForeignKey(a => a.RegulatoryApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.PIApproverUser)
            .WithMany()
            .HasForeignKey(a => a.PIApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Specimen>()
            .HasIndex(s => s.BarcodeID)
            .IsUnique();

        modelBuilder.Entity<Specimen>()
            .HasIndex(s => new { s.BoxID, s.PositionRow, s.PositionCol })
            .IsUnique();

        modelBuilder.Entity<Specimen>()
            .Property(s => s.Status)
            .HasDefaultValue("In-Stock");

        modelBuilder.Entity<Specimen>()
            .Property(s => s.SpotsShippedInternational)
            .HasDefaultValue(0);

        modelBuilder.Entity<Specimen>()
            .Property(s => s.SpotsReservedLocal)
            .HasDefaultValue(0);

        modelBuilder.Entity<Specimen>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Specimens_Status",
                "\"Status\" IN ('In-Stock', 'Staged', 'Shipped', 'Missing', 'Depleted', 'Discarded', 'Temp', 'Not Yet Received')"));

        modelBuilder.Entity<Specimen>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Specimens_AliquotNumber",
                "\"AliquotNumber\" IS NULL OR \"AliquotNumber\" IN (1, 2)"));

        modelBuilder.Entity<Specimen>()
            .HasOne(s => s.DiscardApproval)
            .WithMany(a => a.DiscardedSpecimens)
            .HasForeignKey(s => s.DiscardApprovalID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentBatch>()
            .Property(b => b.ImportDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ShipmentBatch>()
            .Property(b => b.Status)
            .HasDefaultValue("Pending Approval");

        modelBuilder.Entity<ShipmentBatch>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ShipmentBatches_Status",
                "\"Status\" IN ('Pending Approval', 'Approved', 'Rejected', 'Processing', 'Shipped')"));

        modelBuilder.Entity<ShipmentBatch>()
            .HasOne(b => b.ImportedByUser)
            .WithMany()
            .HasForeignKey(b => b.ImportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentBatch>()
            .HasOne(b => b.Approval)
            .WithMany(a => a.ShipmentBatches)
            .HasForeignKey(b => b.ApprovalID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentRequest>()
            .Property(r => r.Status)
            .HasDefaultValue("Pending");

        modelBuilder.Entity<ShipmentRequest>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ShipmentRequests_Status",
                "\"Status\" IN ('Pending', 'Staged', 'Shipped', 'Not Found', 'Previously Shipped', 'Discarded', 'Not Yet Received')"));

        modelBuilder.Entity<ShipmentRequest>()
            .HasOne(r => r.Batch)
            .WithMany(b => b.ShipmentRequests)
            .HasForeignKey(r => r.BatchID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentRequest>()
            .HasOne(r => r.RequestedSampleType)
            .WithMany(t => t.ShipmentRequests)
            .HasForeignKey(r => r.RequestedSampleTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentRequest>()
            .HasOne(r => r.MatchedSpecimen)
            .WithMany(s => s.ShipmentRequests)
            .HasForeignKey(r => r.MatchedSpecimenID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.Batch)
            .WithMany(b => b.Shipments)
            .HasForeignKey(s => s.BatchID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.ShippedByUser)
            .WithMany()
            .HasForeignKey(s => s.ShippedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.ShippedBox)
            .WithMany(b => b.Shipments)
            .HasForeignKey(s => s.ShippedBoxID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentContent>()
            .HasOne(sc => sc.Shipment)
            .WithMany(s => s.ShipmentContents)
            .HasForeignKey(sc => sc.ShipmentID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShipmentContent>()
            .HasOne(sc => sc.Specimen)
            .WithMany(s => s.ShipmentContents)
            .HasForeignKey(sc => sc.SpecimenID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FilterPaperUsage>()
            .Property(f => f.UsageDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<FilterPaperUsage>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_FilterPaperUsages_SpotsUsed",
                "\"SpotsUsed\" BETWEEN 1 AND 4"));

        modelBuilder.Entity<FilterPaperUsage>()
            .HasOne(f => f.Specimen)
            .WithMany(s => s.FilterPaperUsages)
            .HasForeignKey(f => f.SpecimenID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FilterPaperUsage>()
            .HasOne(f => f.UsedByUser)
            .WithMany()
            .HasForeignKey(f => f.UsedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FilterPaperUsage>()
            .HasOne(f => f.ShipmentContent)
            .WithMany()
            .HasForeignKey(f => f.ShipmentContentID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.TableName, a.RecordID })
            .HasDatabaseName("IX_AuditLog_TableRecord");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Timestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
