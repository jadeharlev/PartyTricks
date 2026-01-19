using Game;
using UnityEngine;

namespace Services {
    public class PlayerServiceBootstrapper : MonoBehaviour {
        [Header("Service Prefabs")]
        [SerializeField] private GameObject PlayerServicePrefab;
        [SerializeField] private GameObject GameSessionManagerPrefab;

        [Header("Config")] 
        [SerializeField] private bool persistAcrossScenes = true;

        private void Awake() {
            InitializePlayerServices();
        }

        private void InitializePlayerServices() {
            if (ServiceLocatorAccessor.GetService<IPlayerService>() != null) {
                DebugLogger.Log(LogChannel.Systems, "Player system already initialized.", LogLevel.Verbose);
                return;
            }
            
            DebugLogger.Log(LogChannel.Systems, "Initializing player services", LogLevel.Verbose);

            
            var playerServiceGameObject = SetUpPlayerService(out var playerService);
            var sessionManagerGameObject = SetUpGameSessionManager();

            if (persistAcrossScenes) {
                ServiceLocatorAccessor.RegisterGlobal<IPlayerService>(playerService);
                DontDestroyOnLoad(playerServiceGameObject);
                DontDestroyOnLoad(sessionManagerGameObject);
            }
            else {
                ServiceLocatorAccessor.Register<IPlayerService>(playerService);
            }
            
            DebugLogger.Log(LogChannel.Systems, "Services registered", LogLevel.Verbose);
        }

        private GameObject SetUpGameSessionManager() {
            GameObject sessionManagerGameObject;
            if (GameSessionManagerPrefab != null) {
                sessionManagerGameObject = Instantiate(GameSessionManagerPrefab);
            }
            else {
                sessionManagerGameObject = new GameObject("GameSessionManager");
                sessionManagerGameObject.AddComponent<GameSessionManager>();
                sessionManagerGameObject.AddComponent<UnityEngine.InputSystem.PlayerInputManager>();
            }

            return sessionManagerGameObject;
        }

        private GameObject SetUpPlayerService(out IPlayerService playerService) {
            GameObject playerServiceGameObject;
            if (PlayerServicePrefab != null) {
                playerServiceGameObject = Instantiate(PlayerServicePrefab);
            } else {
                playerServiceGameObject = new GameObject("PlayerService");
                playerServiceGameObject.AddComponent<PlayerService>();
            }
            
            playerService = playerServiceGameObject.GetComponent<IPlayerService>();
            return playerServiceGameObject;
        }
    }
}