using System;

namespace VineSwinging.Core {
    public class AirborneState : IPlayerState {
        private readonly PlayerStateMachine playerStateMachine;

        public AirborneState(PlayerStateMachine playerStateMachine) {
            this.playerStateMachine = playerStateMachine;
        }
        public void Enter(PlayerContext playerContext, SwingConfig swingConfig) {
            playerContext.CurrentStateType = PlayerStateType.Airborne;
            playerContext.SwingAngle = 0f;
        }

        public void Update(PlayerContext playerContext, SwingConfig swingConfig, float deltaTime, bool releasePressed) {
            playerContext.VelocityY -= swingConfig.Gravity * deltaTime;
            playerContext.PositionX += playerContext.VelocityX * deltaTime;
            playerContext.PositionY += playerContext.VelocityY * deltaTime;
            int minVineIndex = playerContext.CurrentVineIndex + 1;
            var grabPosition = GrabEvaluator.CheckGrab(playerContext.PositionX, playerContext.PositionY, playerStateMachine.VineXPositions,
                playerStateMachine.VineAnchorY, swingConfig, minVineIndex, playerStateMachine.VinePhaseOffsets, playerStateMachine.VinePeriods, playerStateMachine.ElapsedTime);
            bool grabbed = grabPosition != -1;
            if (grabbed) {
                playerContext.CurrentVineIndex = grabPosition;
                if(grabPosition > playerContext.FurthestVineIndex) {
                    playerContext.FurthestVineIndex = grabPosition;
                }
                playerContext.PendingEvents.Add(PlayerEvent.GrabbedVine);
                playerStateMachine.TransitionTo(new SwingingState(playerStateMachine));
            }

            if (playerContext.PositionY < swingConfig.FallThresholdY) {
                playerStateMachine.TransitionTo(new FallingState(playerStateMachine));
            }
        }

        public void Exit(PlayerContext playerContext) { }
    }
}