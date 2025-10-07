using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button returnToMenuButton;
    private PauseManager pauseManager;
    private InputAction cancelAction;

    public void Initialize(PauseManager manager) {
        pauseManager = manager;
        if (resumeButton != null) {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (optionsButton != null) {
            optionsButton.onClick.AddListener(OnOptionsClicked);
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        SetUpNavigation();
        SelectFirstButton();
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
    }

    private void Update() {
        if (cancelAction != null && cancelAction.WasPressedThisFrame()) {
            OnResumeClicked();
        }
    }

    private void SelectFirstButton() {
        if (resumeButton != null) {
            resumeButton.Select();
            EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    private void SetUpNavigation() {
        if (resumeButton != null) {
            var navigation = resumeButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = resumeButton;
            navigation.selectOnDown = optionsButton;
            resumeButton.navigation = navigation;
        }
        if (resumeButton != null) {
            var navigation = optionsButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = resumeButton;
            navigation.selectOnDown = returnToMenuButton;
            optionsButton.navigation = navigation;
        }
        if (resumeButton != null) {
            var navigation = returnToMenuButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = optionsButton;
            navigation.selectOnDown = returnToMenuButton;
            returnToMenuButton.navigation = navigation;
        }
        
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
            resumeButton.onClick.RemoveListener(OnResumeClicked);
        }

        if (optionsButton != null) {
            optionsButton.onClick.RemoveListener(OnOptionsClicked);
        }

        if (returnToMenuButton != null) {
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
        }
        cancelAction = null;
    }
}
