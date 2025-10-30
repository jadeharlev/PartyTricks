using System.Collections.Generic;
using UnityEngine;

public class CardDeck {
    

    private List<Card> cards;

    private List<CardSuits> suits =
        new List<CardSuits>
        {
            CardSuits.Clubs,
            CardSuits.Diamonds,
            CardSuits.Spades,
            CardSuits.Hearts
        };
    
    public bool IsEmpty => cards.Count == 0;
    
    private void ShuffleList<T>(ref List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public CardDeck() {
        int cardValue = 1;
        cards = new List<Card>();
        foreach (CardSuits suit in suits) {
            for (int i = 1; i <= 13; i++) {
                cards.Add(new Card(suit, i));
            }
        }
        ShuffleList(ref cards);
    }

    public Card DrawCard() {
        if (cards.Count == 0) {
            return new Card(CardSuits.Undefined, -1);
        }
        Card cardToReturn = cards[^1];
        cards.RemoveAt(cards.Count - 1);
        return cardToReturn;
    }
}
