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
        if(decks.Count == 0) Initialize();
        Card cardToReturn;
        do {
            if(decks.Count == 0) Initialize();
            var shoeIndex = GetRandomShoeIndex();
            cardToReturn = decks[shoeIndex].DrawCard();
            RemoveDeckIfEmpty(shoeIndex);
        } while (!cardToReturn.IsReal && decks.Count > 0);
        
        if (!cardToReturn.IsReal) {
            Initialize();
            return DrawCard();
        }
        return cardToReturn;
    }

    private void RemoveDeckIfEmpty(int shoeIndex) {
        if (shoeIndex < decks.Count && decks[shoeIndex].IsEmpty) {
            decks.RemoveAt(shoeIndex);
        }
    }

    private int GetRandomShoeIndex() {
        return Random.Range(0, decks.Count);
    }
}
