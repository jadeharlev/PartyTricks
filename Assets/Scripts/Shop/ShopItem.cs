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
    [SerializeField] public string PowerUpEffectId;
    [Tooltip("For internal use only.")] public bool IsImplemented = false;

    public ItemDefinition ToDefinition() {
        PowerUpEffect effect = PowerUpRegistry.CreateEffect(PowerUpEffectId);
        return new ItemDefinition(Icon, Id, DisplayName, Cost, Category, Description,
        IsTemporary, NumberOfUsesIfApplicable, effect);
    }
}
