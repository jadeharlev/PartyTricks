using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopItemsDisplay : MonoBehaviour {
    [SerializeField] private ShopItemUI[] ShopItemUIElements;
    private ShopItem[] powerups;
    private ShopItem emptyPowerup;
    private List<ShopItem> selectedPowerups;

    private void Awake() {
        powerups = Resources.LoadAll<ShopItem>("Powerups");
        emptyPowerup = Resources.Load<ShopItem>("SpecialPowerups/EmptyItem");
        selectedPowerups = new List<ShopItem>();
    }

    public void Reset() {
        selectedPowerups.Clear();
    }

    public void SetUpItems() {
        for (var i = 0; i < ShopItemUIElements.Length - 1; i++) {
            var shopItemElement = ShopItemUIElements[i];
            ShopItem powerup;
            powerup = GetRandomPowerup();
            shopItemElement.SetItem(powerup);
            selectedPowerups.Add(powerup);
        }
        ShopItemUIElements[^1].SetItem(emptyPowerup);
    }

    private ShopItem GetRandomPowerup() {
        var available = powerups.Where(powerup => selectedPowerups.All(item => item.Id != powerup.Id)).ToList();
        if (available.Count == 0) {
            Debug.Log("Warning: no powerups were available.");
            return emptyPowerup;
        }
        return available[Random.Range(0, available.Count)];
    }
}
