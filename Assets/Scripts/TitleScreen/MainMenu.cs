using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Options;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using STOP_MODE = FMOD.Studio.STOP_MODE;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TitleScreen {
    public class MainMenu : MonoBehaviour, ITitleScreenPhase {
        [SerializeField] private EventReference musicEvent;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text connectedPlayersLabel;
        [SerializeField] private MenuSoundConfigSO menuSoundConfig;
        [SerializeField] private GameObject creditsScreenPrefab;

        private EventInstance musicInstance;
        private Button[] buttons;
        private int focusedIndex;
        private float lastNavigateTime;
        private const float NavigationCooldownSeconds = 0.2f;
        private IPlayerService playerService;
        private readonly List<InputAction> subscribedSubmitActions = new();
        private readonly List<InputAction> gamepadNavigateActions = new();
        private InputSystemUIInputModule inputModule;
        private InputActionReference cachedMoveAction;
        private InputActionReference cachedSubmitAction;
        
        public event Action OnPhaseComplete;

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
        }

        private void Start()
        {
            if (creditsScreenPrefab != null) {
                Instantiate(creditsScreenPrefab);
            }
            
            if (!musicEvent.IsNull) {
                musicInstance.start();
            }
            else {
                Debug.LogWarning("Error: Music is missing");
            }

            startGameButton.onClick.AddListener(StartGame);
            optionsButton.onClick.AddListener(ShowOptions);
            creditsButton.onClick.AddListener(ShowCredits);
            quitButton.onClick.AddListener(QuitGame);

            buttons = new [] {
                startGameButton,
                optionsButton,
                creditsButton,
                quitButton
            };

            foreach (var playerSlot in playerService.PlayerSlots) {
                if(playerSlot.IsOccupied && !playerSlot.IsAI) HandleInputConnected(playerSlot.PlayerInput);
            }

            inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (inputModule != null) {
                cachedMoveAction = inputModule.move;
                cachedSubmitAction = inputModule.submit;
                inputModule.move = null;
                inputModule.submit = null;
            }

            StartCoroutine(FocusFirstButtonAfterOneFrame());
        }

        private void OnEnable() {
            playerService.OnInputConnected += HandleInputConnected;
            SubscribeToGamepadActions();
            UpdateConnectedPlayersText();
        }
        

        private void SubscribeToGamepadActions() {
            foreach (var playerInput in PlayerInput.all) {
                SubscribeGamepadActionsForInput(playerInput);
            }
        }

        private void SubscribeGamepadActionsForInput(PlayerInput playerInput) {
            if (playerInput == null) return;
            if (playerInput.devices.Any(device => device is Keyboard || device is Mouse)) return;

            var submitAction = playerInput.actions.FindAction("UI/Submit");
            if (submitAction != null) {
                submitAction.performed += OnGamepadSubmitPerformed;
                subscribedSubmitActions.Add(submitAction);
            }

            var navigateAction = playerInput.actions.FindAction("UI/Navigate");
            if (navigateAction != null) {
                gamepadNavigateActions.Add(navigateAction);
            }
        }

        private void OnGamepadSubmitPerformed(InputAction.CallbackContext context) {
            if (focusedIndex < 0 || focusedIndex >= buttons.Length) return;
            buttons[focusedIndex].onClick.Invoke();
        }

        private void Update() {
            if (Time.time - lastNavigateTime < NavigationCooldownSeconds) return;

            foreach (var action in gamepadNavigateActions) {
                var movementVector = action.ReadValue<Vector2>();
                if (Mathf.Abs(movementVector.y) < 0.5f || Mathf.Abs(movementVector.y) <= Mathf.Abs(movementVector.x)) continue;

                if (movementVector.y > 0) {
                    focusedIndex = Mathf.Max(0, focusedIndex - 1);
                } else {
                    focusedIndex = Mathf.Min(buttons.Length - 1, focusedIndex + 1);
                }
                EventSystem.current.SetSelectedGameObject(buttons[focusedIndex].gameObject);
                RuntimeManager.PlayOneShot(menuSoundConfig.HighlightSound);
                lastNavigateTime = Time.time;
                break;
            }
        }

        private void HandleInputConnected(PlayerInput playerInput) {
            SubscribeGamepadActionsForInput(playerInput);
            UpdateConnectedPlayersText();
        }

        private void UpdateConnectedPlayersText() {
            int deviceCount = PlayerInput.all.Count;
            connectedPlayersLabel.text = deviceCount > 0 ?
                $"{deviceCount} controller{(deviceCount == 1 ? "" : "s")} connected"
                : string.Empty;
        }

        private void QuitGame() {
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private IEnumerator FocusFirstButtonAfterOneFrame() {
            yield return null;
            FocusFirstButton();
        }

        private void FocusFirstButton() {
            if (startGameButton != null) {
                EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);
            }
            focusedIndex = 0;
        }

        private void StartGame() {
            OnPhaseComplete?.Invoke();
        }

        private void ShowOptions() {
            FindFirstObjectByType<OptionsMenu>()?.Show();
        }
        
        private void ShowCredits() {
            FindFirstObjectByType<CreditsScreen>()?.Show();
        }

        private void OnDisable() {
            playerService.OnInputConnected -= HandleInputConnected;
            
            foreach (var action in subscribedSubmitActions) {
                action.performed -= OnGamepadSubmitPerformed;
            }
        
            subscribedSubmitActions.Clear();
            gamepadNavigateActions.Clear();
            
            if (musicInstance.isValid()) {
                musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        private void OnDestroy() {
            startGameButton.onClick.RemoveAllListeners();
            optionsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        
            if (inputModule != null) {
                inputModule.move = cachedMoveAction;
                inputModule.submit = cachedSubmitAction;
            }
            
            musicInstance.release();
        }
    }
}
