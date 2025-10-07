using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private bool pauseIsEnabled = true;
    public bool IsPaused { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetUpCanvas();
        SetUpInputAction();
    }

    private void SetUpCanvas() {
        GameObject canvasObject = new GameObject("PauseCanvas");
        canvasObject.transform.SetParent(transform);
        pauseMenuCanvas = canvasObject.AddComponent<Canvas>();
        pauseMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseMenuCanvas.sortingOrder = 1000;
        var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void SetUpInputAction() {
        pauseAction = InputSystem.actions.FindAction("UI/Pause");
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        submitAction = InputSystem.actions.FindAction("UI/Submit");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        clickAction = InputSystem.actions.FindAction("UI/Click");
        if (pauseAction == null) {
            Debug.LogError("PauseManager: Could not find UI pause action.");
        }
    }

    private void Update() {
        if (pauseAction != null && pauseAction.WasPressedThisFrame() && pauseIsEnabled) {
            if (IsPaused) {
                Resume();
            }
            else {
                Pause();
            }
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
        }
        InputSystem.actions.FindActionMap("Player")?.Disable();
        
        if (pauseMenuPrefab != null) {
            activePauseMenu = Instantiate(pauseMenuPrefab, pauseMenuCanvas.transform);
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
    }
}
