using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging.States {
    public class VineSwingingGameplayState : IVineSwingingGameState {
        private readonly VineSwingingMinigameManager minigameManager;

        public VineSwingingGameplayState(VineSwingingMinigameManager minigameManager) {
            this.minigameManager = minigameManager;
        }
        
        public void Enter() {
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Entered Gameplay State.");
            minigameManager.IsInGameplay = true;
            minigameManager.GameTimer.OnTimerEnd += OnTimerEnd;
            minigameManager.GameTimer.StartTimer();
        }

        public void OnUpdate() {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < minigameManager.PlayerStateMachines.Length; i++) {
                PlayerSlot slot = minigameManager.PlayerService.PlayerSlots[i];
                bool releasePressed;

                if (slot.IsAI) {
                    releasePressed = AIAutoRelease(minigameManager.PlayerStateMachines[i]);
                }
                else {
                    releasePressed = slot.InputHandler.SelectIsPressed();
                }
                
                minigameManager.PlayerStateMachines[i].Update(deltaTime, releasePressed);
                
                minigameManager.PlayerViews[i].Pull(minigameManager.PlayerStateMachines[i].PlayerContext);

                var playerContext = minigameManager.PlayerStateMachines[i].PlayerContext;
                var swingConfig = minigameManager.PlayerStateMachines[i].SwingConfig;
                int score = playerContext.FurthestVineIndex * swingConfig.VineScoreValue + playerContext.TotalCoinValue;
                minigameManager.PlayerCornerDisplays[i].UpdateScore(score);
            }
        }

        private bool AIAutoRelease(PlayerStateMachine stateMachine) {
            if (stateMachine.PlayerContext.CurrentStateType != PlayerStateType.Swinging) return false;
            // TODO make this better
            return stateMachine.PlayerContext.SwingPhase > 1.5f;
        }

        private void OnTimerEnd() {
            minigameManager.GameTimer.OnTimerEnd -= OnTimerEnd;
            minigameManager.ChangeState(new VineSwingingResultsState(minigameManager));
        }

        public void Exit() {
            minigameManager.IsInGameplay = false;
            DebugLogger.Log(LogChannel.Systems, $"VineSwinging: Exited Gameplay State.");
        }
    }
}