using Game;
using UnityEngine;

namespace Services {
    public class MainBootstrapper : MonoBehaviour {
        private GameBootstrapConfig config;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeGame() {
            GameObject bootstrapper = new GameObject("MainBootstrapper");
            bootstrapper.AddComponent<MainBootstrapper>().Initialize();
            DontDestroyOnLoad(bootstrapper);
        }

        private void Initialize() {
            LoadConfig();
            // order matters for dependency reasons
            CreatePlayerService();
            CreateEconomyService();
            CreatePauseService();
            CreatePowerUpService();
            CreateGameFlowManager();
            CreateGameSessionManager();
        }

        private void LoadConfig() {
            config = Resources.Load<GameBootstrapConfig>(GameBootstrapConfig.ResourcePath);
            if (config == null) {
                Debug.LogError($"Missing Game Bootstrap Config at Resources/{GameBootstrapConfig.ResourcePath}");
            }
        }

        private void CreatePlayerService() {
            GameObject playerServiceObject;
            if (config != null && config.PlayerServicePrefab != null) {
                playerServiceObject = Instantiate(config.PlayerServicePrefab, transform);
            }
            else {
                playerServiceObject = new GameObject("PlayerService");
                playerServiceObject.transform.SetParent(transform);
                playerServiceObject.AddComponent<PlayerService>();
            }
            var service = playerServiceObject.GetComponent<IPlayerService>();
            if (service != null) {
                ServiceLocatorAccessor.RegisterGlobal<IPlayerService>(service);
            }
            else {
                Debug.LogError($"Missing IPlayerService component!");
            }
        }

        private void CreateEconomyService() {
            GameObject economyServiceObject;
            if (config != null && config.EconomyServicePrefab != null) {
                economyServiceObject = Instantiate(config.EconomyServicePrefab, transform);
            }
            else {
                economyServiceObject = new GameObject("EconomyService");
                economyServiceObject.transform.SetParent(transform);
                economyServiceObject.AddComponent<EconomyService>();
            }
            var service = economyServiceObject.GetComponent<IEconomyService>();
            if (service != null) {
                ServiceLocatorAccessor.RegisterGlobal<IEconomyService>(service);
            }
            else {
                Debug.LogError($"Missing IEconomyService component!");
            }
        }

        private void CreatePauseService() {
            GameObject pauseServiceObject;
            if (config != null && config.PauseServicePrefab != null) {
                pauseServiceObject = Instantiate(config.PauseServicePrefab, transform);
            }
            else {
                pauseServiceObject = new GameObject("PauseService");
                pauseServiceObject.transform.SetParent(transform);
                pauseServiceObject.AddComponent<PauseService>();
            }
            var service = pauseServiceObject.GetComponent<IPauseService>();
            if (service != null) {
                ServiceLocatorAccessor.RegisterGlobal<IPauseService>(service);
            }
            else {
                Debug.LogError($"Missing IPauseService component!");
            }
        }
        
        private void CreatePowerUpService() {
            // no prefab here since there's no extra configuration necessary
            GameObject powerUpServiceObject = new GameObject("PowerUpService");
            powerUpServiceObject.transform.SetParent(transform);
            powerUpServiceObject.AddComponent<PowerUpService>();
            var service = powerUpServiceObject.GetComponent<IPowerUpService>();
            ServiceLocatorAccessor.RegisterGlobal<IPowerUpService>(service);
        }

        private void CreateGameFlowManager() {
            GameObject gameFlowServiceObject;
            if (config != null && config.GameFlowManagerPrefab != null) {
                gameFlowServiceObject = Instantiate(config.GameFlowManagerPrefab, transform);
            }
            else {
                gameFlowServiceObject = new GameObject("GameFlowService");
                gameFlowServiceObject.transform.SetParent(transform);
                gameFlowServiceObject.AddComponent<GameFlowManager>();
            }
            var service = gameFlowServiceObject.GetComponent<IGameFlowService>();
            if (service != null) {
                ServiceLocatorAccessor.RegisterGlobal<IGameFlowService>(service);
            }
            else {
                Debug.LogError($"Missing IGameFlowService component!");
            }
        }

        private void CreateGameSessionManager() {
            GameObject gameSessionManagerObject;
            if (config != null && config.GameSessionManagerPrefab != null) {
                gameSessionManagerObject = Instantiate(config.GameSessionManagerPrefab, transform);
            }
            else {
                gameSessionManagerObject = new GameObject("GameSessionManager");
                gameSessionManagerObject.transform.SetParent(transform);
                gameSessionManagerObject.AddComponent<UnityEngine.InputSystem.PlayerInputManager>();
                gameSessionManagerObject.AddComponent<GameSessionManager>();
                Debug.LogWarning($"Missing GameSessionManager prefab!");
            }
        }
    }
}