using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCornerDisplay : MonoBehaviour {
    [SerializeField] private GameObject PowerupLayoutGroup;
    [SerializeField] private TMP_Text FundsLabel;
    [SerializeField] private GameObject MiniPowerupPrefab;

    private Wallet wallet;
    private Inventory inventory;
    private bool isInitialized = false;

    public void Initialize(PlayerProfile profile) {
        CleanupSubscriptions();
        wallet = profile.Wallet;
        inventory = profile.Inventory;
        
        wallet.OnFundsChanged += UpdateFunds;
        inventory.OnItemAdded += AddItem;
        isInitialized = true;

        RefreshDisplay();
    }

    private void RefreshDisplay() {
        if (!isInitialized) {
            return;
        }

        ClearItemsDisplay();
        foreach (var item in inventory.Items) {
            AddItem(item);
        }
        UpdateFunds(wallet.GetCurrentFunds());
    }

    private void ClearItemsDisplay() {
        for (int i = PowerupLayoutGroup.transform.childCount - 1; i >= 0; i--) {
            Destroy(PowerupLayoutGroup.transform.GetChild(i).gameObject);
        }
    }

    private void UpdateFunds(int funds) {
        FundsLabel.text = "funds: " + funds;
    }

    private void AddItem(ItemDefinition item) {
        if (item.Id == "emptyItem") return;
        GameObject newItem = Instantiate(MiniPowerupPrefab);
        newItem.transform.SetParent(PowerupLayoutGroup.transform);
        newItem.transform.localScale = Vector3.one;
        Image imageComponent = newItem.GetComponent<Image>();
        imageComponent.sprite = item.Image;
    }

    private void CleanupSubscriptions() {
        if (wallet != null) wallet.OnFundsChanged -= UpdateFunds;
        if (inventory != null) inventory.OnItemAdded -= AddItem;
    }

    private void OnDestroy() {
        CleanupSubscriptions();
    }
}
