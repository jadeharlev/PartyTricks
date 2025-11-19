using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardProgressDisplay : MonoBehaviour {
    [SerializeField] private MinigameIconMappingSO IconMapping;
    private List<(Texture2D texture, bool isDouble)> minigameList = new();
    [SerializeField] private GameObject ProgressBoardIconPrefab;
    [SerializeField] private GameObject DoubleProgressBoardIconPrefab;
    private void Start() {
        if (GameFlowManager.Instance == null) {
            Debug.LogWarning("GameFlowManager not instantiated!");
        }

        PopulateMinigameList();
        UpdateDisplay();
    }

    private void PopulateMinigameList() {
        AddCompletedMinigames();
        AddUpcomingMinigames();
    }

    private void AddUpcomingMinigames() {
        List<(MinigameType minigameType, bool isDouble)> upcomingMinigames = GameFlowManager.Instance.GetUpcomingMinigameList();
        foreach (var minigame in upcomingMinigames) {
            minigameList.Add((IconMapping.GetIcon(minigame.minigameType, minigame.isDouble, false), minigame.isDouble));
        }
    }

    private void AddCompletedMinigames() {
        List<(MinigameType minigameType, bool isDouble)> completedMinigames = GameFlowManager.Instance.GetCompletedMinigameList();
        foreach (var minigame in completedMinigames) {
            minigameList.Add((IconMapping.GetIcon(minigame.minigameType, minigame.isDouble, true), minigame.isDouble));
        }
    }

    private void UpdateDisplay() {
        foreach (var minigame in minigameList) {
            Texture2D texture = minigame.texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            GameObject prefab = ProgressBoardIconPrefab;
            if (minigame.isDouble) {
                prefab = DoubleProgressBoardIconPrefab;
            }

            GameObject instantiatedIcon = Instantiate(prefab, transform);
            instantiatedIcon.GetComponent<Image>().sprite = sprite;
        }
    }
}
