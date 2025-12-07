public class DireDodgingCountdownState : IDireDodgingState {
    private MinigameStartCountdown minigameStartCountdown;

    public DireDodgingCountdownState(MinigameStartCountdown minigameStartCountdown) {
        this.minigameStartCountdown = minigameStartCountdown;
    }
    
    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Countdown State.", LogLevel.Verbose);
        DireDodgingMinigameManager.Instance.SetMusicIntensity(0);
        minigameStartCountdown.StartTimer();
        minigameStartCountdown.OnTimerEnd += OnTimerEnd;
    }
    
    public void OnUpdate() {}

    private void OnTimerEnd() {
        minigameStartCountdown.OnTimerEnd -= OnTimerEnd;
        DireDodgingMinigameManager.Instance.StartMusic();
        DireDodgingMinigameManager.Instance.TransitionToGameplay();
    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Countdown State.", LogLevel.Verbose);
    }
}