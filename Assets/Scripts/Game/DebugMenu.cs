using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugMenu : MonoBehaviour {
    private bool shouldShowMenu = false;
    private InputAction toggleDebugMenuAction;
    private Rect windowRect = new Rect(20, 20, 300, 400);
    private bool isDoubleRound = false;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        toggleDebugMenuAction = InputSystem.actions.FindAction("UI/ToggleDebugMenu");
        
        if (toggleDebugMenuAction == null) {
            Debug.LogWarning("DebugMenu: ToggleDebugMenu action not found. Debug menu will not be toggleable.");
        }
    }

    private void Update() {
        if (toggleDebugMenuAction != null && toggleDebugMenuAction.WasPressedThisFrame()) {
            shouldShowMenu = !shouldShowMenu;
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
        
        if (GUILayout.Button("Load Test Minigame", GUILayout.Height(40))) {
            LoadTestMinigame();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Shop", GUILayout.Height(40))) {
            SceneManager.LoadScene("Shop");
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Main Menu", GUILayout.Height(40))) {
            SceneManager.LoadScene("MainMenu");
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
        
        if (GUILayout.Button("Reset All Player Funds", GUILayout.Height(30))) {
            ResetAllPlayerFunds();
        }

        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }

    private static void DisplayPlayerFunds() {
        if (GameSessionManager.Instance != null) {
            GUILayout.Space(5);
            for (int i = 0; i < GameSessionManager.Instance.PlayerSlots.Length; i++) {
                var slot = GameSessionManager.Instance.PlayerSlots[i];
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

    private void LoadTestMinigame() {
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Loading Test Minigame. Double: {isDoubleRound}");
        SceneManager.LoadScene("TestMinigameScene");
        SceneManager.sceneLoaded += OnTestMinigameSceneLoaded;
    }

    private void OnTestMinigameSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "TestMinigame") return;
        
        SceneManager.sceneLoaded -= OnTestMinigameSceneLoaded;
        
        TestMinigameManager manager = FindAnyObjectByType<TestMinigameManager>();
        if (manager != null) {
            manager.Initialize(isDoubleRound);
            DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Test minigame manager initialized. Double: {isDoubleRound}");
        } else {
            Debug.LogError("Debug Menu: Could not find TestMinigameManager in scene!");
        }
    }

    private void AddFundsToAllPlayers(int amount) {
        if (GameSessionManager.Instance == null) {
            Debug.LogWarning("Debug Menu: GameSessionManager not found.");
            return;
        }

        foreach (var slot in GameSessionManager.Instance.PlayerSlots) {
            if (slot?.Profile != null) {
                slot.Profile.Wallet.AddFunds(amount);
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, $"Debug Menu: Added {amount} funds to all players.");
    }

    private void ResetAllPlayerFunds() {
        if (GameSessionManager.Instance == null) {
            Debug.LogWarning("Debug Menu: GameSessionManager not found.");
            return;
        }

        foreach (var slot in GameSessionManager.Instance.PlayerSlots) {
            if (slot?.Profile != null) {
                slot.Profile.Wallet.Reset();
            }
        }
        
        DebugLogger.Log(LogChannel.Systems, "Debug Menu: Reset all player funds to 200.");
    }
}