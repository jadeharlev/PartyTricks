using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour {
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text category;
    [SerializeField] private TMP_Text description;
    [SerializeField] ShopPointers pointers;
    
    public void SetItem(ShopItem item) {
        icon.sprite = item.Icon;
        name.text = item.DisplayName;
        cost.text = "Cost: " + item.Cost;
        category.text = item.Category.ToString();
        description.text = item.Description;
        if (item.Id == "emptyItem") {
            SetEmpty();
        }
    }

    private void SetEmpty() {
        cost.gameObject.SetActive(false);
        category.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
    }

    public void OnPointedTo(int playerIndex, bool shouldShow, bool shouldBeLocked) {
        pointers.OnPointedTo(playerIndex, shouldShow, shouldBeLocked);
    }
}
