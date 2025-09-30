using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject {
    public Sprite Icon;
    public string Id;
    public string DisplayName;
    public int Cost;
    public PowerUpCategory Category;
    public string Description;
    public bool IsTemporary;
    public int NumberOfUsesIfApplicable;
    [SerializeField]
    public PowerUpEffect PowerUpEffect;

    public ItemDefinition ToDefinition() => new ItemDefinition(Icon, Id, DisplayName, Cost, Category, Description, IsTemporary, NumberOfUsesIfApplicable, PowerUpEffect);
}
