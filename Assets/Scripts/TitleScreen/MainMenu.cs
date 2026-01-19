using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    private UIDocument mainMenu;

    private Button startGameButton;
    private Button optionsButton;
    private Button quitButton;
    private bool hasFocused;
    private InputAction navigateAction;
    private IGameFlowService gameFlowService;

    private void Awake() {
        gameFlowService = ServiceLocatorAccessor.GetService<IGameFlowService>();
    }

    private void Start()
    {
        VisualElement root = mainMenu.rootVisualElement;
        startGameButton = root.Query<Button>("StartGameButton");
        optionsButton = root.Query<Button>("OptionsButton");
        quitButton = root.Query<Button>("QuitButton");
        quitButton.clicked += QuitGame;
        startGameButton.clicked += StartGame;
        optionsButton.clicked += ShowOptions;
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        StartCoroutine(FocusFirstButtonAfterOneFrame());
    }

    private void QuitGame() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void Update() {
        if (!hasFocused && navigateAction.ReadValue<Vector2>() != Vector2.zero) {
            FocusFirstButton();
        }
    }

    private IEnumerator FocusFirstButtonAfterOneFrame() {
        yield return null;
        FocusFirstButton();
    }

    private void FocusFirstButton() {
        if (startGameButton != null) {
            startGameButton.Focus();
        }

        hasFocused = true;
    }

    private void StartGame() {
        if (gameFlowService != null) {
            gameFlowService.StartGame();
        }
        else {
            Debug.LogError("MainMenu: GameFlowManager not found.");
        }
    }

    private void ShowOptions() {
        Debug.Log("NOT IMPLEMENTED YET");
        throw new System.NotImplementedException();
    }
}
