namespace VineSwinging.Core {
    public class PlayerStateMachine {
        public PlayerContext PlayerContext { get; }
        public SwingConfig SwingConfig { get; }
        public float[] VineXPositions { get; }
        public float VineAnchorY { get; }
        public float[] VinePhaseOffsets { get; }
        public float ElapsedTime { get; private set; }
        public float[] VinePeriods { get; }
        
        private IPlayerState currentState;

        public PlayerStateMachine(SwingConfig swingConfig, float[] vineXPositions, float vineAnchorY, float[] vinePhaseOffsets, float[] vinePeriods) {
            SwingConfig = swingConfig;
            VineXPositions = vineXPositions;
            VineAnchorY = vineAnchorY;
            VinePhaseOffsets = vinePhaseOffsets;
            VinePeriods = vinePeriods;
            PlayerContext = new PlayerContext();
        }

        public void Start(int initialVineIndex) {
            PlayerContext.CurrentVineIndex = initialVineIndex;
            PlayerContext.FurthestVineIndex = initialVineIndex;
            PlayerContext.PositionX = VineXPositions[initialVineIndex];
            PlayerContext.PositionY = VineAnchorY;
            TransitionTo(new SwingingState(this));
        }

        public void Update(float deltaTime, bool releasePressed) {
            ElapsedTime += deltaTime;
            currentState?.Update(PlayerContext, SwingConfig, deltaTime, releasePressed);
        }

        public void TransitionTo(IPlayerState newState) {
            currentState?.Exit(PlayerContext);
            currentState = newState;
            currentState.Enter(PlayerContext, SwingConfig);
        }
    }
}