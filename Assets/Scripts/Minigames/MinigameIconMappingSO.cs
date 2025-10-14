using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameIconMapping", menuName = "Scriptable Objects/Minigame Icon Mapping")]
public class MinigameIconMappingSO : ScriptableObject {
    [System.Serializable]
    public class IconEntry {
        public MinigameType Type;
        public Texture2D NormalIcon;
        public Texture2D DoubleIcon;
    }

    [SerializeField] private List<IconEntry> icons = new();

    public Texture2D GetIcon(MinigameType type, bool isDouble) {
        var entry = icons.Find(x => x.Type == type);
        if (entry == null) {
            Debug.LogError($"Icon missing for type {type}");
            return null;
        }

        if (isDouble) {
            return entry.DoubleIcon;
        }

        return entry.NormalIcon;
    }
}