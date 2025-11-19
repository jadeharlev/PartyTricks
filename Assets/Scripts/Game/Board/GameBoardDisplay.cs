using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameBoardDisplay : MonoBehaviour {
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private MinigameIconMappingSO iconMapping;

    private VisualElement root;
    private VisualElement iconRow;

    public event Action OnContinue;

    private void Awake() {
        root = uiDocument.rootVisualElement;
        iconRow = root.Q<VisualElement>("IconRow");
        root.style.display = DisplayStyle.None;
    }

    public void ShowBoard(List<(MinigameType minigameType, bool IsDouble)> gameBoard) {
        root.style.display = DisplayStyle.Flex;
        DrawIcons(gameBoard);
        StartCoroutine(WaitAndThenContinue());
    }

    private void DrawIcons(List<(MinigameType minigameType, bool IsDouble)> gameBoard) {
        iconRow.Clear();
        List<VisualElement> iconElements = new();
        foreach (var round in gameBoard) {
            Texture2D iconTexture = iconMapping.GetIcon(round.minigameType, round.IsDouble);
            if (iconTexture == null) {
                Debug.LogError($"Missing icon for {round.minigameType} (Double: {round.IsDouble})");
                continue;
            }

            VisualElement iconElement = new VisualElement();
            iconElement.AddToClassList("minigame-icon");
            iconElement.style.opacity = 0f;
            Image iconImage = new Image
            {
                image = iconTexture,
                scaleMode = ScaleMode.ScaleToFit
            };
            iconElement.Add(iconImage);
            iconRow.Add(iconElement);
            iconElements.Add(iconElement);
        }
        StartCoroutine(FadeGameIconsIn(iconElements));
    }

    private IEnumerator FadeGameIconsIn(List<VisualElement> icons) {
        foreach (var icon in icons) {
            float animationDuration = 0.2f;
            float elapsed = 0f;
            while (elapsed < animationDuration) {
                elapsed += Time.deltaTime;
                icon.style.opacity = Mathf.Clamp01(elapsed / animationDuration);
                yield return null;
            }
            icon.style.opacity = 1f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator WaitAndThenContinue() {
        yield return new WaitForSeconds(10);
        OnContinue?.Invoke();
    }
}
