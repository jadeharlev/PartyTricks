using TMPro;
using UnityEngine;

public class BlackjackDealerDisplay : MonoBehaviour {
    [SerializeField] 
    private TMP_Text CardValueLabel;
    [SerializeField]
    private BlackjackCardDisplay BlackjackCardDisplay;

    public void UpdateCardValueLabel(int lowCardValue, int highCardValue, string resultText = null) {
        string text;
        if (lowCardValue == highCardValue) {
            text = $"Value: {lowCardValue}";
        }
        else {
            text = $"Value: {lowCardValue} ({highCardValue})";
        }

        if (!string.IsNullOrEmpty(resultText)) text += $" ({resultText})";
        CardValueLabel.text = text;
    }
    
    public void AddCard(Card card) {
        BlackjackCardDisplay.AddCard(card);
    }
}
