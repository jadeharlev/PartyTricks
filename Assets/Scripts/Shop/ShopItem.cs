using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject {
    public Sprite Icon;
    public string Id;
    public string DisplayName;
    public int Cost;
    public PowerUpCategory Category;
    public string Description;
    public bool isTemporary;
    public int NumberOfUsesIfApplicable;
    [SerializeField]
    public PowerUpEffect PowerUpEffect;
}
