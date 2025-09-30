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

    public void Initialize(PlayerProfile profile) {
        wallet = profile.Wallet;
        inventory = profile.Inventory;
        wallet.OnFundsChanged += UpdateFunds;
        inventory.OnItemAdded += AddItem;
        UpdateFunds(wallet.GetCurrentFunds());
        foreach (var item in inventory.Items) {
            AddItem(item);
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

    private void OnDestroy() {
        if (wallet != null) wallet.OnFundsChanged -= UpdateFunds;
        if (inventory != null) inventory.OnItemAdded -= AddItem;
    }
}
