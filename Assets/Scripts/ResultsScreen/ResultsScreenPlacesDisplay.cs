using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsScreenPlacesDisplay : MonoBehaviour {
    [SerializeField] private GameObject DisplayPanel;
    [SerializeField] private TMP_Text FirstPlaceText;
    [SerializeField] private TMP_Text SecondPlaceText;
    [SerializeField] private TMP_Text ThirdPlaceText;
    [SerializeField] private TMP_Text FourthPlaceText;

    public void UpdatePlaces(int[] playerFunds) {
        var rankedResults = playerFunds.Select((funds, index) =>
            new
            {
                Player = index + 1,
                Funds = funds
            }).OrderByDescending(player => player.Funds).ToList();
        FirstPlaceText.text = $"Player {rankedResults[0].Player}: {rankedResults[0].Funds} funds";
        SecondPlaceText.text = $"Player {rankedResults[1].Player}: {rankedResults[1].Funds} funds";
        ThirdPlaceText.text = $"Player {rankedResults[2].Player}: {rankedResults[2].Funds} funds";
        FourthPlaceText.text = $"Player {rankedResults[3].Player}: {rankedResults[3].Funds} funds";
    }

    public void Show() {
        DisplayPanel.SetActive(true);
    }
}
