using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour {
    [SerializeField] private GameObject SuspensePanel;
    [SerializeField] private TMP_Text WinnerLabel;
    [SerializeField] private TMP_Text WinnerNumberLabel;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private ResultsScreenPlacesDisplay ResultsScreenPlacesDisplay;
    private int playerWinnerIndex;
    private int[] playerFunds;

    public void ReturnToMainMenu() {
        ResetProfiles();
        SceneManager.LoadScene("MainMenu");
    }

    private void ResetProfiles() {
        PlayerSlot[] playerSlots = GameSessionManager.Instance.PlayerSlots;
        foreach (var playerSlot in playerSlots) {
            playerSlot.Profile.Reset();
        }
    }

    private void Start() {
        playerFunds = new int[4] {0,0,0,0};
        WinnerLabel.text = "The winner is";
        GetWinner();
        StartCoroutine(WaitAndDisplayWinner());
    }

    private void GetWinner() {
        int max = Int32.MinValue;
        for (int i = 0; i < 4; i++) {
            playerFunds[i] = GameSessionManager.Instance.PlayerSlots[i].Profile.Wallet.GetCurrentFunds();
            if (playerFunds[i] > max) {
                max = playerFunds[i];
                playerWinnerIndex = i;
            } else if (playerFunds[i] == max) {
                Debug.Log("Warning: two players had the same value. Consider investigating.");
            }
        }
    }

    private IEnumerator WaitAndDisplayWinner() {
        yield return new WaitForSeconds(0.5f);
        WinnerLabel.text = "The winner is.";
        yield return new WaitForSeconds(1.3f);
        WinnerLabel.text = "The winner is..";
        yield return new WaitForSeconds(1.5f);
        WinnerLabel.text = "The winner is...";
        yield return new WaitForSeconds(2.5f);
        DisplayWinner();
        yield return new WaitForSeconds(3.5f);
        ResultsScreenPlacesDisplay.UpdatePlaces(playerFunds);
        HideSuspensePanel();
        ResultsScreenPlacesDisplay.Show();
    }

    private void HideSuspensePanel() {
        SuspensePanel.SetActive(false);
    }

    private void DisplayWinner() {
        WinnerNumberLabel.text = "Player " + (playerWinnerIndex+1) + "!";
        Color backgroundColor = playerWinnerIndex switch
        {
            0 => new Color(153 / 256f, 0, 0, 1),
            1 => new Color(66 / 256f, 99 / 256f, 217 / 256f, 1),
            2 => new Color(140 / 256f, 25 / 256f, 212 / 256f, 1),
            3 => new Color(10 / 256f, 121 / 256f, 41 / 256f, 1),
            _ => default
        };

        BackgroundImage.color = backgroundColor;
        WinnerNumberLabel.color = Color.white;
        WinnerLabel.color = Color.white;
    }
}
