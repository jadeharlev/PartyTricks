using System.Collections.Generic;
using Services;
using UnityEngine;

public class ShopPurchaseService {
    public void ResolvePurchases(List<ShopSlotSelector> shopSlotSelectors, ShopItemUI[] shopItems) {
        foreach (var playerSelector in shopSlotSelectors) {
            ProcessPurchase(shopItems, playerSelector);
        }
    }

    private static void ProcessPurchase(ShopItemUI[] shopItems, ShopSlotSelector playerSelector) {
        int index = playerSelector.CurrentShopItemIndex;
        IPlayerService playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        playerSelector.Lock();
        playerSelector.CanAct = false;

        PlayerProfile profile = playerService.GetPlayerProfile(playerSelector.PlayerIndex);
        ShopItemUI item = shopItems[index];
        
        int cost = item.GetItemCost();
        int finalCost = cost;
        foreach (var itemDefinition in profile.Inventory.Items) {
            if (itemDefinition.PowerUpEffect != null) {
                finalCost = itemDefinition.PowerUpEffect.ApplyCostDiscount(finalCost);
            }
        }
        bool purchaseSuccess = profile.Wallet.Buy(finalCost);
        if (purchaseSuccess) {
            profile.Inventory.AddItem(item.GetItem().ToDefinition());
        }
        else {
            Debug.Log("Shop.cs: Player " + playerSelector.PlayerIndex + " could not afford " + shopItems[index].ToString());
        }
    }
}