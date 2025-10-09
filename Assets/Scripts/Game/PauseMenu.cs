using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour {
    private Button resumeButton;
    private Button optionsButton;
    private Button returnToMenuButton;
    private PauseManager pauseManager;
    private InputAction cancelAction;
    private InputAction navigateAction;
    private bool hasFocused;
    [SerializeField] private UIDocument pauseMenu;

    public void Initialize(PauseManager manager) {
        VisualElement root = pauseMenu.rootVisualElement;
        pauseManager = manager;
        resumeButton = root.Q<Button>("ResumeButton");
        optionsButton = root.Q<Button>("OptionsButton");
        returnToMenuButton = root.Q<Button>("ReturnToMenuButton");
        if (resumeButton != null) {
            resumeButton.clicked += OnResumeClicked;
        }

        if (optionsButton != null) {
            optionsButton.clicked += OnOptionsClicked;
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.clicked += OnReturnToMenuClicked;
        }
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void Update() {
        if (cancelAction != null && cancelAction.WasPressedThisFrame()) {
            OnResumeClicked();
        }
        if (!hasFocused && navigateAction.ReadValue<Vector2>() != Vector2.zero) {
            FocusFirstButton();
        }
    }
    
    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    private void FocusFirstButton() {
        if (resumeButton != null) {
            resumeButton.Focus();
        }

        hasFocused = true;
    }
    

    private void OnReturnToMenuClicked() {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
        pauseManager.Resume();
    }

    private void OnOptionsClicked() {
        Debug.Log("Options not yet implemented.");
    }

    private void OnResumeClicked() {
        pauseManager.Resume();
    }

    private void OnDestroy() {
        if (resumeButton != null) {
            resumeButton.clicked -= OnResumeClicked;
        }

        if (optionsButton != null) {
            optionsButton.clicked -= OnOptionsClicked;
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.clicked -= OnReturnToMenuClicked;
        }
        cancelAction = null;
    }
}
