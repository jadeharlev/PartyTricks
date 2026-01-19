using System;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game {
    public class GameSessionManager : MonoBehaviour {
        private IPlayerService playerService;
        private UnityEngine.InputSystem.PlayerInputManager unityInputManager;
        
        // used to handle cleanup
        private bool isQuitting = false;
    
        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            unityInputManager = GetComponent<UnityEngine.InputSystem.PlayerInputManager>();

            if (playerService == null) {
                DebugLogger.Log(LogChannel.Systems, "No PlayerService found", LogLevel.Error);
                enabled = false;
                return;
            }
            
            if (unityInputManager == null) {
                DebugLogger.Log(LogChannel.Systems, "No PlayerInputManager found", LogLevel.Error);
                enabled = false;
                return;
            }

            ConfigureInputManager();
        }

        private void ConfigureInputManager() {
            unityInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
            unityInputManager.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        private void OnEnable() {
            unityInputManager.onPlayerJoined += HandlePlayerJoined;
            unityInputManager.onPlayerLeft += HandlePlayerLeft;
        }
        
        private void OnDisable() {
            unityInputManager.onPlayerJoined -= HandlePlayerJoined;
            unityInputManager.onPlayerLeft -= HandlePlayerLeft;
        }
        
        public void HandlePlayerJoined(PlayerInput playerInput) {
            Debug.Log($"[GameSessionManager] Unity PlayerInput detected: {playerInput.playerIndex}");
            bool playerJoined = playerService.TryJoinPlayer(playerInput);
            if (!playerJoined) {
                Debug.LogWarning("[GameSessionManager] Failed to join player");
            }
        }
        
        public void HandlePlayerLeft(PlayerInput playerInput) {
            if (isQuitting) return;
            Debug.Log($"[GameSessionManager] Unity PlayerInput left: {playerInput.playerIndex}");
            for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
                var slot = playerService.PlayerSlots[i];
                if (slot.PlayerInput == playerInput) {
                    playerService.RemovePlayer(i);
                    break;
                }
            }
        }

        private void OnApplicationQuit() {
            isQuitting = true;
        }
    }
}