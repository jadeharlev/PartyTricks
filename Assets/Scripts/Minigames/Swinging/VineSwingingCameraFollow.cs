using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineSwingingCameraFollow : MonoBehaviour {
        [SerializeField] private float smoothTimeInSeconds = 0.3f;
        [SerializeField] private float lookAheadDistance = 2f;

        private PlayerContext targetContext;
        private float fixedYPosition;
        private float fixedZPosition;
        private float xVelocity;

        public void Initialize(PlayerContext context) {
            targetContext = context;
            fixedYPosition = transform.localPosition.y;
            fixedZPosition = transform.localPosition.z;
        }

        private void LateUpdate() {
            if (targetContext == null) return;
            float targetXPosition = targetContext.PositionX + lookAheadDistance;
            float smoothedXPosition =
                Mathf.SmoothDamp(transform.localPosition.x, targetXPosition, ref xVelocity, smoothTimeInSeconds);
            transform.localPosition = new Vector3(smoothedXPosition, fixedYPosition, fixedZPosition);
        }
    }
}