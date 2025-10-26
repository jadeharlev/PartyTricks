using System.Collections.Generic;
using UnityEngine;

public class BlackjackShoe {
    private int ShoeSize = 6;
    private List<CardDeck> decks;
    
    public BlackjackShoe() {
        Initialize();
    }

    private void Initialize() {
        decks = new();
        for (int i = 0; i < ShoeSize; i++) {
            decks.Add(new CardDeck());
        }
    }

    public Card DrawCard() {
        Card cardToReturn = new Card(CardSuits.Undefined, -1);
        do {
            var shoeIndex = GetRandomShoeIndex();
            cardToReturn = decks[shoeIndex].DrawCard();
            RemoveDeckIfEmpty(shoeIndex);
        } while (!cardToReturn.IsReal && decks.Count >= 1);
        
        if (!cardToReturn.IsReal) {
            this.Initialize();
            return DrawCard();
        }
        return cardToReturn;
    }

    private void RemoveDeckIfEmpty(int shoeIndex) {
        if (decks[shoeIndex].IsEmpty) {
            decks.RemoveAt(shoeIndex);
        }
    }

    private int GetRandomShoeIndex() {
        return Random.Range(0, decks.Count);
    }
}
