using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Data;

public class ConstructionDbContext : DbContext
{
    public ConstructionDbContext(DbContextOptions<ConstructionDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for all entities
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<SiteLocation> SiteLocations => Set<SiteLocation>();
    public DbSet<RFI> RFIs => Set<RFI>();
    public DbSet<Submittal> Submittals => Set<Submittal>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<CostItem> CostItems => Set<CostItem>();
    public DbSet<ChangeOrder> ChangeOrders => Set<ChangeOrder>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Risk> Risks => Set<Risk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConstructionDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedDate = DateTime.UtcNow;
            }
        }
    }
}
