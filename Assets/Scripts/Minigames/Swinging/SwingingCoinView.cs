using System;
using UnityEngine;

namespace Minigames.Swinging {
    public class SwingingCoinView : MonoBehaviour {
        private int coinValue;
        private bool isCollected;

        public void Initialize(int coinValue) {
            this.coinValue = coinValue;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (isCollected) return;
            var playerView = other.GetComponentInParent<VineSwingingPlayerView>();
            if (playerView == null) return;
            isCollected = true;
            playerView.CollectCoin(coinValue);
            Destroy(gameObject);
        }
    }
}