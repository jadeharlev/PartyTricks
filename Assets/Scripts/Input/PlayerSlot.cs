using UnityEngine;

public class PlayerSlot : MonoBehaviour {
    [SerializeField] private int playerIndex;
    public IShopNavigator Navigator { get; private set; }
    public bool IsAI { get; private set; }
    public int Funds { get; set; } = 100;
    // public Inventory PlayerInventory { get; private set; }

    public void Initialize(int index, IShopNavigator shopNavigator, bool isAI) {
        playerIndex = index;
        Navigator = shopNavigator;
        IsAI = isAI;
        
        // if (PlayerInventory == null) PlayerInventory = new Inventory();
    }

    public void ReplaceShopNavigator(IShopNavigator shopNavigator, bool isAI) {
        this.Navigator = shopNavigator;
        this.IsAI = isAI;
    }
}
