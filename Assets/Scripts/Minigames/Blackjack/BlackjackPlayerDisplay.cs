using System;
using TMPro;
using UnityEngine;

public class BlackjackPlayerDisplay : MonoBehaviour {
    [SerializeField]
    private TMP_Text PlayerNumberLabel;
    [SerializeField]
    private TMP_Text BetLabel;
    [SerializeField]
    private TMP_Text CardValueLabel;
    [SerializeField]
    private BlackjackCardDisplay BlackjackCardDisplay;
    [SerializeField]
    private BlackjackControlsDisplay BlackjackControlsDisplay;

    public void UpdatePlayerNumberDisplay(int playerIndex) {
        PlayerNumberLabel.text = $"P{playerIndex + 1}";
    }

    public void UpdateBetLabel(int bet, int playerFunds) {
        BetLabel.text = $"Bet: {bet} / {playerFunds}";
    }

    public void UpdateForEndOfGame(int lowCardValue, int highCardValue, int dealerValue, int amountWonOrLost, bool gotBlackjack = false) {
        string result;
        
        int playerBest = highCardValue > 21 ? lowCardValue : highCardValue;
        bool playerBust = playerBest > 21;
        bool dealerBust = dealerValue > 21;

        if (gotBlackjack) {
            result = "Blackjack!";
        } else switch (playerBust) {
            case true when dealerBust:
                result = "Both Busted!";
                break;
            case true:
                result = "Busted!";
                break;
            default: {
                if (dealerBust) {
                    result = "Dealer Bust!";
                } else if (playerBest > dealerValue) {
                    result = "Won!";
                } else if (playerBest < dealerValue) {
                    result = "Lost.";
                }
                else {
                    result = "Push.";
                }

                break;
            }
        }
        
        UpdateCardValueLabel(lowCardValue, highCardValue, result);

        if (amountWonOrLost == 0) BetLabel.text = "No change.";
        else if (amountWonOrLost > 0) BetLabel.text = $"Won {amountWonOrLost}!";
        else
        {
            amountWonOrLost = Math.Abs(amountWonOrLost);
            BetLabel.text = $"Lost {amountWonOrLost}.";
        }
    }

    public void UpdateCardValueLabel(int lowCardValue, int highCardValue, string resultText = null) {
        string text;
        if (lowCardValue == highCardValue) {
            text = $"Card Value: {lowCardValue}";
        }
        else {
            text = $"Card Value: {lowCardValue} ({highCardValue})";
        }
        
        if (!string.IsNullOrEmpty(resultText)) text += $"\n{resultText}";
        
        CardValueLabel.text = text;
    }

    public void AddCard(Card card) {
        BlackjackCardDisplay.AddCard(card);
    }

    public void HideControls() {
        BlackjackControlsDisplay.HideControls();
    }

    public void ShowControls() {
        BlackjackControlsDisplay.ShowControls();
    }

    public void ChangeToPlayerColor(Color color) {
        PlayerNumberLabel.color = color;
        BetLabel.color = color;
        CardValueLabel.color = color;
    }
}
