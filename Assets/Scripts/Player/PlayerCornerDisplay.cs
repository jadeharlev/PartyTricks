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
    private DisplayMode displayMode;

    public void Initialize(PlayerProfile profile, DisplayMode mode = DisplayMode.Funds) {
        CleanupSubscriptions();
        wallet = profile.Wallet;
        inventory = profile.Inventory;

        displayMode = mode;
        
        if (mode == DisplayMode.Funds) {
            wallet.OnFundsChanged += UpdateFunds;
        }
        else if(mode == DisplayMode.Score) {
            UpdateScore(0);
        } else if (mode == DisplayMode.Eliminations) {
            UpdateEliminations(0);
        }
        inventory.OnItemAdded += AddItem;
        isInitialized = true;

        RefreshDisplay();
    }

    public void UpdateEliminations(int eliminations, int outPlace = -1) {
        if (outPlace == -1) {
            FundsLabel.text = "eliminations: " + eliminations;
        }
        else {
            FundsLabel.text = GetPlaceString(outPlace) + " | eliminations: " + eliminations;
        }
    }

    private string GetPlaceString(int outPlace) {
        return outPlace switch
        {
            1 => "1st place",
            2 => "2nd place",
            3 => "3rd place",
            4 => "4th place",
            _ => "ERR place"
        };
    }

    private void RefreshDisplay() {
        if (!isInitialized) {
            return;
        }

        ClearItemsDisplay();
        foreach (var item in inventory.Items) {
            AddItem(item);
        }
        if(displayMode == DisplayMode.Funds) UpdateFunds(wallet.GetCurrentFunds());
    }

    private void ClearItemsDisplay() {
        for (int i = PowerupLayoutGroup.transform.childCount - 1; i >= 0; i--) {
            Destroy(PowerupLayoutGroup.transform.GetChild(i).gameObject);
        }
    }

    private void UpdateFunds(int funds) {
        FundsLabel.text = "funds: " + funds;
    }

    public void UpdateScore(int score) {
        FundsLabel.text = "score: " + score;
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
        if (wallet != null && displayMode == DisplayMode.Funds) wallet.OnFundsChanged -= UpdateFunds;
        if (inventory != null) inventory.OnItemAdded -= AddItem;
    }

    private void OnDestroy() {
        CleanupSubscriptions();
    }

    public enum DisplayMode {
        Funds,
        Score,
        Eliminations
    }
}
