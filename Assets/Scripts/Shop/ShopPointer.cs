using UnityEngine;
using UnityEngine.UI;

public class ShopPointer : MonoBehaviour {
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private Sprite pointerIcon;
    [SerializeField] private Image imageComponent;
    private bool isLocked = false;

    public void SetLocked() {
        if (isLocked) return;
        isLocked = true;
        imageComponent.sprite = lockedIcon;
    }

    public void SetUnlocked() {
        if (!isLocked) return;
        isLocked = false;
        imageComponent.sprite = pointerIcon;
    }
}
