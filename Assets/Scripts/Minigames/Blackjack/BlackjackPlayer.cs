using System.Collections.Generic;

public class BlackjackPlayer {
    public List<Card> cards;
    public bool GotBlackjack { get; private set; }
    
    public BlackjackPlayer() {
        cards = new List<Card>();
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
}
