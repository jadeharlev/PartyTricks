public struct Card {
    private CardSuits suit { get; }
    // 1 is ace, 13 is king
    public int Value { get; private set; }

    public bool IsReal => suit != CardSuits.Undefined;

    public bool IsFaceCard => (Value >= 11);

    public Card(CardSuits suit, int value) {
        this.suit = suit;
        this.Value = value;
    }
}
