using System.Collections.Generic;
using UnityEngine;

public class BlackjackPlayer {
    private readonly List<Card> cards;
    private int numberOfOddsPowerups;
    public bool GotBlackjack { get; private set; }
    
    public BlackjackPlayer(int numberOfOddsPowerups = 0) {
        cards = new List<Card>();
        this.numberOfOddsPowerups = numberOfOddsPowerups;
    }

    public void DrawCard(Card card) {
        cards.Add(card);

        if (cards.Count == 2) {
            if (GetBestValue() == 21) GotBlackjack = true;
        }
    }

    public int GetLowValue() {
        int handValue = 0;
        foreach (var card in cards) {
            if (card.Value == 1) {
                handValue++;
            }
            else if (card.IsFaceCard) {
                handValue += 10;
            }
            else {
                handValue += card.Value;
            }
        }
        return handValue;
    }

    public int GetHighValue() {
        int handValue = 0;
        foreach (var card in cards) {
            if (card.Value == 1) {
                handValue += 11;
            }
            else if (card.IsFaceCard) {
                handValue += 10;
            }
            else {
                handValue += card.Value;
            }
        }
        return handValue;
    }

    public int GetBestValue() {
        int highValue = GetHighValue();
        if (highValue > 21) return GetLowValue();
        return highValue;
    }

    public bool HasBusted() {
        return GetLowValue() > 21;
    }

    public bool TryToProtect() {
        if (numberOfOddsPowerups > 0) {
            numberOfOddsPowerups--;
        }
        else {
            return false;
        }

        if (Random.Range(1, 11) <= 5) {
            Debug.Log("Odds powerup activated and saved player.");
            return true;
        }
        Debug.Log("Odds powerup did not activate.");
        return false;
    }
}
