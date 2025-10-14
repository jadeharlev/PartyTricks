using System;

public interface IMinigameManager {
    // triggered when minigame concludes
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    // is this a double length / double points round?
    public bool IsDoubleRound { get; }
}