using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseService : MonoBehaviour, IPauseService
{
    [SerializeField] private GameObject pauseMenuPrefab;
    private GameObject activePauseMenu;
    
    private List<InputAction> playerPauseActions = new();
    private Dictionary<int, InputAction> playerIndexToPauseAction = new();
    
    private bool pauseIsEnabled = true;
    private IPlayerService playerService;
    public bool IsPaused { get; private set; }
    
    public event Action OnPause;
    public event Action OnUnpause;

    private void Awake() {
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        SetUpInputActions();
        SubscribeToEvents();
    }
    

    private void SetUpInputActions() {
        var uiMap = InputSystem.actions.FindActionMap("UI");
        if (uiMap == null) {
            Debug.LogWarning("PauseService: uiMap is null.");
        }
    }

    private void SubscribeToEvents() {
        if (playerService != null) {
            playerService.OnPlayerJoined += HandlePlayerJoined;
            playerService.OnPlayerLeft += HandlePlayerLeft;
        }

        RegisterExistingPlayers();
    }

    private void RegisterExistingPlayers() {
        for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
            var slot = playerService.PlayerSlots[i];
            if (!slot.IsAI && slot.InputHandler != null) {
                var playerInput = slot.PlayerInput;
                if (playerInput != null) {
                    RegisterPlayerPauseAction(i, playerInput);
                }
            }
        }
    }
    
    private void HandlePlayerJoined(int playerIndex, PlayerProfile profile) {
        var slot = playerService.PlayerSlots[playerIndex];
        if (slot != null && !slot.IsAI) {
            var playerInput = slot.PlayerInput;
            if (playerInput != null) {
                RegisterPlayerPauseAction(playerIndex, playerInput);
            }
        }
    }
    
    private void HandlePlayerLeft(int playerIndex) {
        if (playerIndexToPauseAction.TryGetValue(playerIndex, out var pauseAction)) {
            playerPauseActions.Remove(pauseAction);
            playerIndexToPauseAction.Remove(playerIndex);
        }
    }

    private void RegisterPlayerPauseAction(int playerIndex, PlayerInput playerInput) {
        var pauseAction = playerInput.actions.FindAction("Pause");
        if (pauseAction == null) {
            var uiMap = playerInput.actions.FindActionMap("UI");
            pauseAction = uiMap?.FindAction("Pause");
        }

        if (pauseAction != null && !playerPauseActions.Contains(pauseAction)) {
            playerPauseActions.Add(pauseAction);
            playerIndexToPauseAction[playerIndex] = pauseAction;
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
    public void DoTimedPause(float lengthInSeconds, Action onComplete) {
        StartCoroutine(Hitstop(lengthInSeconds, onComplete));
    }

    private IEnumerator Hitstop(float lengthInSeconds, Action onComplete) {
        Time.timeScale = 0f;
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + lengthInSeconds) {
            yield return null;
        }
        Time.timeScale = 1f;
        onComplete?.Invoke();
    }

    public void Pause() {
        if (IsPaused) return;
        IsPaused = true;
        OnPause?.Invoke();
        Time.timeScale = 0f;

        EnableUIInput();
        
        if (pauseMenuPrefab != null) {
            activePauseMenu = Instantiate(pauseMenuPrefab);
            var pauseMenu = activePauseMenu.GetComponent<PauseMenu>(); 
            pauseMenu?.Initialize(this);
        }
        
        DebugLogger.Log(LogChannel.Systems, "Game paused.");
    }

    private void EnableUIInput() {
        var uiMap = InputSystem.actions.FindActionMap("UI");
        var playerMap = InputSystem.actions.FindActionMap("Player");
        uiMap.Enable();
        playerMap.Disable();

        foreach (var pauseAction in playerPauseActions) {
            pauseAction?.Enable();
        }
    }
    
    public void Resume() {
        if (!IsPaused) return;
        OnUnpause?.Invoke();
        IsPaused = false;
        Time.timeScale = 1f;
        EnableGameInput();
        if (activePauseMenu != null) {
            Destroy(activePauseMenu);
            activePauseMenu = null;
        }
        DebugLogger.Log(LogChannel.Systems, "Game resumed.");
    }

    private void EnableGameInput() {
        var playerMap = InputSystem.actions.FindActionMap("Player");
        playerMap?.Enable();
    }

    private void OnDestroy() {
        Time.timeScale = 1f;
        if (playerService != null) {
            playerService.OnPlayerJoined -= HandlePlayerJoined;
            playerService.OnPlayerLeft -= HandlePlayerLeft;
        }
    }
}
