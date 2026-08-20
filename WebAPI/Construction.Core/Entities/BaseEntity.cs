namespace Construction.Core.Entities;

/// <summary>
/// Base entity with audit fields for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
