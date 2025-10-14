public struct PlayerMinigameResult {
    // zero indexed playerslot index
    public readonly int PlayerIndex;
    // zero indexed (0 is first place)
    private readonly int playerRank;
    // funds earned based on minigame calculation; already includes doubling.
    public readonly int BaseFundsEarned;
    public PlayerMinigameResult(int playerIndex, int playerRank, int baseFundsEarned) {
        PlayerIndex = playerIndex;
        this.playerRank = playerRank;
        BaseFundsEarned = baseFundsEarned;
    }

    public bool PlayerWon => this.playerRank == 0;
    public int PlayerPlace => this.playerRank+1;
}