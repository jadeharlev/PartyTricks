using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineSwingingPlayerView : MonoBehaviour {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private PlayerContext currentPlayerContext;
        
        public void Pull(PlayerContext playerContext) {
            currentPlayerContext = playerContext;
            transform.localPosition = new Vector3(currentPlayerContext.PositionX, currentPlayerContext.PositionY);
            transform.localRotation = Quaternion.Euler(0f, 0f, currentPlayerContext.SwingAngle * Mathf.Rad2Deg);
            spriteRenderer.enabled = (currentPlayerContext.CurrentStateType != PlayerStateType.Falling);
            foreach (var pendingEvent in currentPlayerContext.PendingEvents) {
                // TODO Handle event
            }
            currentPlayerContext.ClearEvents();
        }

        public void CollectCoin(int value) {
            if (currentPlayerContext == null) return;
            currentPlayerContext.TotalCoinValue += value;
            currentPlayerContext.PendingEvents.Add(PlayerEvent.CollectedCoin);
        }
    }
}