using TMPro;
using UnityEngine;

public class CoinTiltPlacesDisplay : MonoBehaviour {
    [SerializeField] private TMP_Text[] playerPlaceTextObjects;
    [SerializeField] private GameObject placesPanel;

    public void Show() {
        placesPanel.SetActive(true);
    }

    public void Hide() {
        placesPanel.SetActive(false);
    }

    public void UpdateTextObjects(string[] newText) {
        for (int i = 0; i < playerPlaceTextObjects.Length; i++) {
            playerPlaceTextObjects[i].text = newText[i];
        }
    }
}
