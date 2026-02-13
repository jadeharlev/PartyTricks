using System.Collections.Generic;

namespace VineSwinging.Core {
    public enum PlayerStateType { Swinging, Airborne, Falling }
    public enum PlayerEvent { GrabbedVine, Fell, Launched, CollectedCoin }
    public class PlayerContext {
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public float SwingPhase;
        public float SwingAngle;
        public int CurrentVineIndex;
        public int FurthestVineIndex;
        public float RespawnTimer;
        public int TotalCoinValue;
        public PlayerStateType CurrentStateType;
        public List<PlayerEvent> PendingEvents = new();

        public void ClearEvents() => PendingEvents.Clear();
    }
}
