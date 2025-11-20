using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    [SerializeField] private GameObject pauseMenuPrefab;
    private Canvas pauseMenuCanvas;
    private GameObject activePauseMenu;
    private InputAction pauseAction;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;
    private InputAction clickAction;
    private InputAction pointAction;
    private InputAction toggleDebugMenuAction;
    private List<InputAction> playerPauseActions = new List<InputAction>();
    private bool pauseIsEnabled = true;
    public bool IsPaused { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetUpInputAction();
    }
    

    private void SetUpInputAction() {
        pauseAction = InputSystem.actions.FindAction("UI/Pause");
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        submitAction = InputSystem.actions.FindAction("UI/Submit");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        clickAction = InputSystem.actions.FindAction("UI/Click");
        pointAction = InputSystem.actions.FindAction("UI/Point");
        toggleDebugMenuAction = InputSystem.actions.FindAction("UI/ToggleDebugMenu");
        if (pauseAction == null) {
            Debug.LogError("PauseManager: Could not find UI pause action.");
        }
        GameSessionManager.Instance.OnPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput) {
        var playerPauseAction = playerInput.currentActionMap.FindAction("Pause");
        if (playerPauseAction == null) {
            var uiMap = playerInput.actions.FindActionMap("UI");
            if (uiMap != null) {
                playerPauseAction = uiMap.FindAction("Pause");
            }
        }
        
        if (playerPauseAction != null && !playerPauseActions.Contains(playerPauseAction)) {
            playerPauseActions.Add(playerPauseAction);
            DebugLogger.Log(LogChannel.Systems, $"Added pause action for player {playerInput.playerIndex}");
        }
    }

    private void Update() {
        if (!pauseIsEnabled) return;
        foreach (var action in playerPauseActions) {
            if (action != null && action.WasPressedThisFrame()) {
                TogglePause();
                return;
            }
        }
    }

    private void TogglePause() {
        if (IsPaused) {
            Resume();
        }
        else {
            Pause();
        }
    }

    public void EnablePause() {
        pauseIsEnabled = true;
        DebugLogger.Log(LogChannel.Systems, "Pause enabled.");
    }

    public void DisablePause() {
        pauseIsEnabled = false;
        if (IsPaused) {
            Resume();
        }
        DebugLogger.Log(LogChannel.Systems, "Pause disabled.");
    }

    public void Pause() {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;

        var uiActionMap = InputSystem.actions.FindActionMap("UI");
        if (uiActionMap != null) {
            uiActionMap.Disable();
            pauseAction.Enable();
            navigateAction.Enable();
            submitAction.Enable();
            cancelAction.Enable();
            clickAction.Enable();
            pointAction.Enable();
            toggleDebugMenuAction.Enable();
        }
        
        foreach (var action in playerPauseActions) {
            if (action != null) {
                action.Enable();
            }
        }
        
        InputSystem.actions.FindActionMap("Player")?.Disable();
        
        if (pauseMenuPrefab != null) {
            activePauseMenu = Instantiate(pauseMenuPrefab);
            var pauseMenu = activePauseMenu.GetComponent<PauseMenu>();
            if (pauseMenu != null) {
                pauseMenu.Initialize(this);
            }
        }
        else {
            Debug.LogWarning("PauseManager: pauseMenuPrefab is not assigned.");
        }
        
        DisableShopSelectors();
        
        DebugLogger.Log(LogChannel.Systems, "Game paused.");
    }
    
    private void EnableShopSelectors() {
        var selectors = FindObjectsByType<ShopSlotSelector>(FindObjectsSortMode.None);
        foreach (var selector in selectors) {
            selector.CanAct = true;
        }
    }
    
    private void DisableShopSelectors() {
        var selectors = FindObjectsByType<ShopSlotSelector>(FindObjectsSortMode.None);
        foreach (var selector in selectors) {
            selector.CanAct = false;
        }

        Debug.Log("Disabling shop selectors. Shop selector length is " + selectors.Length);
    }

    public void Resume() {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        if (activePauseMenu != null) {
            Destroy(activePauseMenu);
            activePauseMenu = null;
        }
        var uiActionMap = InputSystem.actions.FindActionMap("UI");
        if (uiActionMap != null) {
            uiActionMap.Enable();
        }

        StartCoroutine(EnableShopSlotSelectorsAfterOneFrame());
        DebugLogger.Log(LogChannel.Systems, "Game resumed.");
    }

    private IEnumerator EnableShopSlotSelectorsAfterOneFrame() {
        yield return null;
        EnableShopSelectors();
    }
    
    public void SetPauseMenuPrefab(GameObject pauseMenuPrefab) {
        this.pauseMenuPrefab = pauseMenuPrefab;
    }

    private void OnDestroy() {
        Time.timeScale = 1f;
        GameSessionManager.Instance.OnPlayerJoined -= OnPlayerJoined;
    }
}
