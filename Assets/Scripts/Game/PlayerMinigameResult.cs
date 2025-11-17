public struct PlayerMinigameResult {
    // zero indexed playerslot index
    public readonly int PlayerIndex;
    // zero indexed (0 is first place)
    private readonly int playerRank;
    // funds earned based on minigame calculation; already includes doubling.
    public readonly int BaseFundsEarned;
    // applicable only in gambling minigames
    public int AmountBet { get; }
    public int Eliminations { get; }
    public PlayerMinigameResult(int playerIndex, int playerRank, int baseFundsEarned, int amountBet = 0, int eliminations = 0) {
        PlayerIndex = playerIndex;
        this.playerRank = playerRank;
        BaseFundsEarned = baseFundsEarned;
        this.AmountBet = amountBet;
        this.Eliminations = eliminations;
    }

    public bool PlayerWon => this.playerRank == 0;
    public int PlayerPlace => this.playerRank+1;
    public int NetFundsEarned => this.BaseFundsEarned - this.AmountBet;
}