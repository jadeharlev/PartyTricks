using System.Collections.Generic;
using DG.Tweening;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO add wait before modifying size after adjusting player count to ensure consistency with KBM input
namespace TitleScreen.GameSetup {
    public class PlayerCountMenu : MonoBehaviour {
        [SerializeField] private RectTransform HumanPlayerRectTransform;
        [SerializeField] private RectTransform AIPlayerRectTransform;
        [SerializeField] private int CanvasWidth = 1920;
        [SerializeField] private Button UpButton;
        [SerializeField] private Image UpArrowImage;
        [SerializeField] private Button DownButton;
        [SerializeField] private Image DownArrowImage;
        [SerializeField] private Button ConfirmButton;
        [SerializeField] private TMP_Text PlayerCountText;
        [SerializeField] private TMP_Text AICountText;
        [SerializeField] private float ScaleAnimationDurationInSeconds = 0.5f;
        [SerializeField] private float FadeAnimationDurationInSeconds = 0.1f;
    
        private IPlayerService playerService;
        private readonly List<InputAction> subscribedSubmitActions = new();
        private readonly List<InputAction> gamepadNavigateActions = new();
    
        private int widthPerPlayer;
        private float rectTransformHeight;
        private int maxPlayers;
        private int currentHumanPlayerCount = 1;

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            maxPlayers = playerService.GetMaxPlayers();
            widthPerPlayer = CanvasWidth / maxPlayers;
            rectTransformHeight = HumanPlayerRectTransform.sizeDelta.y;
        }

        private void OnEnable() {
            UpButton.onClick.AddListener(OnIncreaseInput);
            DownButton.onClick.AddListener(OnDecreaseInput);
            ConfirmButton.onClick.AddListener(OnConfirm);
            playerService.OnInputConnected += SubscribeActionsForInput;
            SubscribeToInputActions();
            UpdatePlayerCount();
        }
        
        private void SubscribeToInputActions() {
            foreach (var playerInput in PlayerInput.all) {
                SubscribeActionsForInput(playerInput);
            }
        }
        
        private void SubscribeActionsForInput(PlayerInput playerInput) {
            if (playerInput == null) return;

            var submitAction = playerInput.actions.FindAction("UI/Submit");
            if (submitAction != null && !subscribedSubmitActions.Contains(submitAction)) {
                submitAction.performed += OnGamepadSubmitPerformed;
                subscribedSubmitActions.Add(submitAction);
            }

            var navigateAction = playerInput.actions.FindAction("UI/Navigate");
            if (navigateAction != null && !gamepadNavigateActions.Contains(navigateAction)) {
                gamepadNavigateActions.Add(navigateAction);
                navigateAction.performed += OnNavigate;
            }
        }

        private void OnGamepadSubmitPerformed(InputAction.CallbackContext callbackContext) {
            OnConfirm();
        }

        private void OnConfirm() {
            Debug.Log("Confirm action triggered.");
        }

        private void OnNavigate(InputAction.CallbackContext callbackContext) {
            float yInput = callbackContext.ReadValue<Vector2>().normalized.y;
            switch (yInput) {
                case 1:
                    OnIncreaseInput();
                    break;
                case -1:
                    OnDecreaseInput();
                    break;
                case 0:
                    break;
            }
        }

        private void OnIncreaseInput() {
            if (currentHumanPlayerCount == maxPlayers) {
                Debug.Log("Player # increase detected, but player count was already at max.");
            }
            else {
                IncrementHumans();
            }
        }
    
        private void IncrementHumans() {
            if (currentHumanPlayerCount == 1) ShowDownArrow(); 
            currentHumanPlayerCount++;
            if (currentHumanPlayerCount == maxPlayers) HideUpArrow();
            AdjustPanelSizeAutomatically();
            UpdatePlayerCount();
        }

        private void ShowDownArrow() {
            DownArrowImage.DOFade(1f, FadeAnimationDurationInSeconds);
        }

        private void HideUpArrow() {
            UpArrowImage.DOFade(0f, FadeAnimationDurationInSeconds);
        }

        private void ShowUpArrow() {
            UpArrowImage.DOFade(1f, FadeAnimationDurationInSeconds);
        }

        private void HideDownArrow() {
            DownArrowImage.DOFade(0f, FadeAnimationDurationInSeconds);
        }

        private void UpdatePlayerCount() {
            PlayerCountText.text = currentHumanPlayerCount.ToString();
            AICountText.text = (maxPlayers - currentHumanPlayerCount).ToString();
        }

        private void AdjustPanelSizeAutomatically() {
            float humanPlayerWidth = widthPerPlayer * currentHumanPlayerCount;
            HumanPlayerRectTransform.DOSizeDelta(new Vector2(humanPlayerWidth, rectTransformHeight), ScaleAnimationDurationInSeconds).SetLink(gameObject);
            AIPlayerRectTransform.DOSizeDelta(new Vector2(CanvasWidth - humanPlayerWidth, rectTransformHeight), ScaleAnimationDurationInSeconds).SetLink(gameObject);
        }

        private void OnDecreaseInput() {
            if (currentHumanPlayerCount == 1) {
                Debug.Log("Player # decrease detected, but player count was already at one.");
            }
            else {
                DecrementHumans();
            }
        }

        private void DecrementHumans() {
            if (currentHumanPlayerCount == maxPlayers) ShowUpArrow(); 
            currentHumanPlayerCount--;
            if (currentHumanPlayerCount == 1) HideDownArrow();
            AdjustPanelSizeAutomatically();
            UpdatePlayerCount();
        }

        private void OnDisable() {
            UpButton.onClick.RemoveListener(OnIncreaseInput);
            DownButton.onClick.RemoveListener(OnDecreaseInput);
            ConfirmButton.onClick.RemoveListener(OnConfirm);
            playerService.OnInputConnected -= SubscribeActionsForInput;
            foreach (var action in subscribedSubmitActions) {
                action.performed -= OnGamepadSubmitPerformed;
            }

            foreach (var action in gamepadNavigateActions) {
                action.performed -= OnNavigate;
            }
            
            subscribedSubmitActions.Clear();
            gamepadNavigateActions.Clear();
        }
    }
}
