using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayingCard : MonoBehaviour {
    [SerializeField] 
    private Image CardImage;
    
    [SerializeField]
    private CardDeckSO cardDeckImages;
    
    // maps card suits to their corresponding sets of cards
    private Dictionary<CardSuits, SuitCards> suitMap;
    
    // maps card values and suits to their appropriate card sprite
    private Dictionary<int, Func<SuitCards, Sprite>> valueSpriteMap;

    private void Awake() {
        InitializeSuitMap();
        InitializeValueSpriteMap();
    }

    private void InitializeValueSpriteMap() {
        valueSpriteMap = new()
        {
            { 1, suit => suit.AceCard },
            { 2, suit => suit.TwoCard },
            { 3, suit => suit.ThreeCard },
            { 4, suit => suit.FourCard },
            { 5, suit => suit.FiveCard },
            { 6, suit => suit.SixCard },
            { 7, suit => suit.SevenCard },
            { 8, suit => suit.EightCard },
            { 9, suit => suit.NineCard },
            { 10, suit => suit.TenCard },
            { 11, suit => suit.JackCard },
            { 12, suit => suit.QueenCard },
            { 13, suit => suit.KingCard }
        };
    }

    private void InitializeSuitMap() {
        suitMap = new() {
            { CardSuits.Clubs, cardDeckImages.ClubCards },
            { CardSuits.Diamonds, cardDeckImages.DiamondCards },
            { CardSuits.Hearts, cardDeckImages.HeartCards },
            { CardSuits.Spades, cardDeckImages.SpadeCards }
        };
    }


    public void SetCard(Card card) {
        if (!suitMap.TryGetValue(card.Suit, out SuitCards suit)) {
            Debug.LogError($"Error: invalid card suit {card.Suit}");
            return;
        }

        if (!valueSpriteMap.TryGetValue(card.Value, out Func<SuitCards, Sprite> getSprite)) {
            Debug.LogError($"Error: invalid card value {card.Value}");
            return;
        }
        CardImage.sprite = getSprite(suit);
    }
}
