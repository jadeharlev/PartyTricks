using System.Collections.Generic;
using UnityEngine;

public class ShopPurchaseService {
    public void ResolvePurchases(List<ShopSlotSelector> shopSlotSelectors, ShopItemUI[] shopItems) {
        Debug.Log("Shop closed! Resolving purchases.");
        foreach (var playerSelector in shopSlotSelectors) {
            ProcessPurchase(shopItems, playerSelector);
        }
    }

    private static void ProcessPurchase(ShopItemUI[] shopItems, ShopSlotSelector playerSelector) {
        int index = playerSelector.CurrentShopItemIndex;
        playerSelector.Lock();
        playerSelector.CanAct = false;
        PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[playerSelector.PlayerIndex];
        PlayerProfile profile = slot.Profile;
        ShopItemUI item = shopItems[index];
        bool purchaseSuccess = profile.Wallet.Buy(item.GetItemCost());
        if (purchaseSuccess) {
            profile.Inventory.AddItem(item.GetItem().ToDefinition());
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " buys " + shopItems[index].ToString());
        }
        else {
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " could not afford " + shopItems[index].ToString());
        }
    }
}