using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameConfigSO", menuName = "Scriptable Objects/Minigame Configuration")]
public class MinigameConfigSO : ScriptableObject
{
    [System.Serializable]
    public class MinigameSceneMapping {
        public MinigameType Type;
        [Tooltip("List of scene names available for minigame type")]
        public List<string> SceneNames;
    }
    
    [SerializeField]
    private List<MinigameSceneMapping> configurations = new List<MinigameSceneMapping>();

    public string GetRandomSceneName(MinigameType type) {
        var minigameMapping = configurations.Find(c => c.Type == type);
        if (minigameMapping == null || minigameMapping.SceneNames.Count == 0) {
            Debug.LogError($"MinigameConfigSO: No scene names were found for minigame type {type}.");
            return null;
        }
        return minigameMapping.SceneNames[Random.Range(0, minigameMapping.SceneNames.Count)];
    }
}
