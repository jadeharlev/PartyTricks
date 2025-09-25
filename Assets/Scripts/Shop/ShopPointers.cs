using UnityEngine;
using UnityEngine.UI;

public class ShopPointers : MonoBehaviour {
    [SerializeField] private ShopPointer[] pointers;

    public void OnPointedTo(int playerIndex, bool shouldShow, bool shouldBeLocked) {
        var pointer = pointers[playerIndex];
        if(!shouldShow) {
            pointer.gameObject.SetActive(false);
            return;
        }
        pointer.gameObject.SetActive(true);
        if (shouldBeLocked) {
            pointer.SetLocked();
        } else pointer.SetUnlocked();
    }
}