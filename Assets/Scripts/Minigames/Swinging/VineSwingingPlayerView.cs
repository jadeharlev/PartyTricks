using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineSwingingPlayerView : MonoBehaviour {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Pull(PlayerContext playerContext) {
            transform.localPosition = new Vector3(playerContext.PositionX, playerContext.PositionY);
            transform.localRotation = Quaternion.Euler(0f, 0f, playerContext.SwingAngle * Mathf.Rad2Deg);
            spriteRenderer.enabled = (playerContext.CurrentStateType != PlayerStateType.Falling);
            foreach (var pendingEvent in playerContext.PendingEvents) {
                // TODO Handle event
            }
            playerContext.ClearEvents();
        }
    }
}