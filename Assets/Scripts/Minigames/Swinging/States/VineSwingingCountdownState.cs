namespace Minigames.Swinging.States {
    public class VineSwingingCountdownState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;
        private readonly MinigameStartCountdown countdown;
        
        public VineSwingingCountdownState(VineSwingingMinigameManager minigameManager, MinigameStartCountdown countdown) {
            this.minigameManager = minigameManager;
            this.countdown = countdown;
        }
        
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Countdown State.");
            countdown.StartTimer();
            countdown.OnTimerEnd += OnCountdownEnd;
        }

        private void OnCountdownEnd() {
            countdown.OnTimerEnd -= OnCountdownEnd;
            minigameManager.ChangeState(new VineSwingingGameplayState(minigameManager));
        }

        public void OnUpdate() {
            
        }

        public void Exit() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Exited Countdown State.");
        }
    }
}