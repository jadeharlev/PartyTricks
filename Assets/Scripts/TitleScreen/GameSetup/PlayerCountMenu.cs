using DG.Tweening;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO add wait before modifying size after adjusting player count to ensure consistency with KBM input
namespace TitleScreen.GameSetup {
    public class PlayerCountMenu : MonoBehaviour {
        [SerializeField] private RectTransform HumanPlayerRectTransform;
        [SerializeField] private int CanvasWidth = 1920;
        [SerializeField] private Button UpButton;
        [SerializeField] private Image UpArrowImage;
        [SerializeField] private Button DownButton;
        [SerializeField] private Image DownArrowImage;
        [SerializeField] private TMP_Text PlayerCountText;
        [SerializeField] private float ScaleAnimationDurationInSeconds = 0.5f;
        [SerializeField] private float FadeAnimationDurationInSeconds = 0.1f;
    
        private IPlayerService playerService;
    
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
        }

        private void AdjustPanelSizeAutomatically() {
            HumanPlayerRectTransform.DOSizeDelta(new Vector2(widthPerPlayer*currentHumanPlayerCount, rectTransformHeight), ScaleAnimationDurationInSeconds);
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
        }
    }
}
