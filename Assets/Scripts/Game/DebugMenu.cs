using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugMenu : MonoBehaviour {
    private static DebugMenu instance;
    private bool shouldShowMenu = false;
    private InputAction toggleDebugMenuAction;
    private Rect windowRect = new Rect(20, 20, 300, 1200);
    private bool isDoubleRound = false;
    private IPlayerService playerService;
    private DireDodgingMinigameManager direDodgingManager;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        toggleDebugMenuAction = InputSystem.actions.FindAction("UI/ToggleDebugMenu");
        
        if (toggleDebugMenuAction == null) {
            Debug.LogWarning("DebugMenu: ToggleDebugMenu action not found. Debug menu will not be toggleable.");
        }
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
    }

    private void Update() {
        if (toggleDebugMenuAction != null && toggleDebugMenuAction.WasPressedThisFrame()) {
            shouldShowMenu = !shouldShowMenu;
        }
        
        if (direDodgingManager != null && SceneManager.GetActiveScene().name != "DireDodging") {
            direDodgingManager = null;
        }
    }

    private void OnGUI() {
        if (!shouldShowMenu) return;
        windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Debug Menu");
    }

    private void DrawDebugWindow(int windowID) {
        GUILayout.BeginVertical();

        GUILayout.Label("Scene Testing", GUI.skin.box);
        GUILayout.Space(10);
        
        isDoubleRound = GUILayout.Toggle(isDoubleRound, "Double Round");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Blackjack", GUILayout.Height(40))) {
            LoadBlackjackScene();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Coin Tilt Minigame", GUILayout.Height(40))) {
            LoadCoinTiltMinigame();
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Load Dire Dodging", GUILayout.Height(40))) {
            LoadDireDodging();
        }

        if (direDodgingManager != null) {
            GUILayout.Label("Dire Dodging Controls", GUI.skin.box);
            GUILayout.Space(10);
        
            if (GUILayout.Button("Kill Player 1", GUILayout.Height(30))) {
                KillDireDodgingPlayer(0);
            }
        
            if (GUILayout.Button("Kill Player 2", GUILayout.Height(30))) {
                KillDireDodgingPlayer(1);
            }
        
            if (GUILayout.Button("Kill Player 3", GUILayout.Height(30))) {
                KillDireDodgingPlayer(2);
            }
        
            if (GUILayout.Button("Kill Player 4", GUILayout.Height(30))) {
                KillDireDodgingPlayer(3);
            }
        
            GUILayout.Space(20);
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Shop", GUILayout.Height(40))) {
            SceneManager.LoadScene("Shop");
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Main Menu", GUILayout.Height(40))) {
            SceneManager.LoadScene("MainMenu");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Results Screen", GUILayout.Height(40))) {
            SceneManager.LoadScene("Results");
        }

        GUILayout.Space(20);

        GUILayout.Label("Game State", GUI.skin.box);
        GUILayout.Space(10);
        
        GUILayout.Label($"Current Scene: {SceneManager.GetActiveScene().name}");
        DisplayPlayerFunds();

        GUILayout.Space(20);

        GUILayout.Label("Utilities", GUI.skin.box);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add 100 to All Players", GUILayout.Height(30))) {
            AddFundsToAllPlayers(100);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Randomize all player funds", GUILayout.Height(30))) {
            RandomizeAllPlayerFunds();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Reset All Player Funds", GUILayout.Height(30))) {
            ResetAllPlayerFunds();
        }

        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
    
            
    private void KillDireDodgingPlayer(int playerIndex) {
        if (direDodgingManager == null) {
            Debug.LogWarning("Debug Menu: Not in Dire Dodging scene.");
            return;
        }
        
        direDodgingManager.DebugKillPlayer(playerIndex);
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Killed Player {playerIndex + 1}");
    }

    private void RandomizeAllPlayerFunds() {
        if (playerService == null) {
            Debug.LogWarning("Debug Menu: PlayerService not found.");
            return;
        }

        foreach (var slot in playerService.PlayerSlots) {
            if (slot?.Profile != null) {
                Wallet wallet = slot.Profile.Wallet;
                int currentFunds = wallet.GetCurrentFunds();
                if(currentFunds > 0) wallet.RemoveFunds(currentFunds);
                wallet.AddFunds(Random.Range(1, 100)*10);
            }
        }
    }

    private void LoadDireDodging() {
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Loading Dire Dodging scene. Double: {isDoubleRound}");
        SceneManager.LoadScene("DireDodging");
        SceneManager.sceneLoaded += OnDireDodgingSceneLoaded;
    }
    
    private void DisplayPlayerFunds() {
        if (playerService != null) {
            GUILayout.Space(5);
            for (int i = 0; i < playerService.GetPlayerCount(); i++) {
                var slot = playerService.PlayerSlots[i];
                if (slot?.Profile != null) {
                    int funds = slot.Profile.Wallet.GetCurrentFunds();
                    string aiLabel = slot.IsAI ? " (AI)" : "";
                    GUILayout.Label($"P{i + 1}: {funds} coins{aiLabel}");
                }
            }
        }
    }

    private void LoadBlackjackScene() {
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Loading Blackjack scene. Double: {isDoubleRound}");
        SceneManager.LoadScene("Blackjack");
        SceneManager.sceneLoaded += OnBlackjackSceneLoaded;
    }

    private void OnBlackjackSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Blackjack") return;
        
        SceneManager.sceneLoaded -= OnBlackjackSceneLoaded;
        
        // Find and initialize the blackjack manager
        BlackjackMinigameManager manager = FindAnyObjectByType<BlackjackMinigameManager>();
        if (manager != null) {
            manager.Initialize(isDoubleRound);
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Blackjack manager initialized. Double: {isDoubleRound}");
        } else {
            Debug.LogError("Debug Menu: Could not find BlackjackMinigameManager in scene!");
        }
    }

    private void LoadCoinTiltMinigame() {
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Loading Coin Tilt Minigame. Double: {isDoubleRound}");
        SceneManager.LoadScene("CoinTiltGame");
        SceneManager.sceneLoaded += OnCoinTiltMinigameSceneLoaded;
    }

    private void OnCoinTiltMinigameSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "CoinTiltGame") return;
        
        SceneManager.sceneLoaded -= OnCoinTiltMinigameSceneLoaded;
        
        CoinTiltMinigameManager manager = FindAnyObjectByType<CoinTiltMinigameManager>();
        if (manager != null) {
            manager.Initialize(isDoubleRound);
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Coin tilt minigame manager initialized. Double: {isDoubleRound}");
        } else {
            Debug.LogError("Debug Menu: Could not find CoinTiltMinigameManager in scene!");
        }
    }
    
    private void OnDireDodgingSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "DireDodging") return;
    
        SceneManager.sceneLoaded -= OnDireDodgingSceneLoaded;
    
        // Find and initialize the dire dodging manager
        direDodgingManager = FindAnyObjectByType<DireDodgingMinigameManager>();
        if (direDodgingManager != null) {
            direDodgingManager.Initialize(isDoubleRound);
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Dire Dodging manager initialized. Double: {isDoubleRound}");
        } else {
            Debug.LogError("Debug Menu: Could not find DireDodgingMinigameManager in scene!");
        }
    }

    private void AddFundsToAllPlayers(int amount) {
        if (playerService == null) {
            Debug.LogWarning("Debug Menu: PlayerService not found.");
            return;
        }

        foreach (var slot in playerService.PlayerSlots) {
            if (slot?.Profile != null) {
                slot.Profile.Wallet.AddFunds(amount);
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Added {amount} funds to all players.");
    }

    private void ResetAllPlayerFunds() {
        if (playerService == null) {
            Debug.LogWarning("Debug Menu: PlayerService not found.");
            return;
        }

        foreach (var slot in playerService.PlayerSlots) {
            if (slot?.Profile != null) {
                slot.Profile.Wallet.Reset();
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, "Debug Menu: Reset all player funds.");
    }
}