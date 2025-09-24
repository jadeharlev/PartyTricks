using UnityEngine;

public class Shop : MonoBehaviour {
    [SerializeField] public ShopItemsDisplay ShopItemDisplay;
    void Start() {
        ShopItemDisplay.SetUpItems();
    }
}
