using UnityEngine;

public class ItemDefinition {
    public Sprite Image;
    public string Id { get; }
    public string DisplayName { get; }
    public int Cost { get; }
    public PowerUpCategory Category { get; }
    public string Description { get; }
    public bool IsTemporary { get; }
    public int NumberOfUsesIfApplicable { get; }
    public PowerUpEffect PowerUpEffect { get; }

    public ItemDefinition(Sprite image, string id, string displayName, int cost, PowerUpCategory category, string description,
        bool isTemporary, int numberOfUsesIfApplicable, PowerUpEffect PowerUpEffect) {
        this.Image = image;
        Id = id;
        DisplayName = displayName;
        Cost = cost;
        Category = category;
        Description = description;
        IsTemporary = isTemporary;
        NumberOfUsesIfApplicable = numberOfUsesIfApplicable;
        this.PowerUpEffect = PowerUpEffect;
    }
}
