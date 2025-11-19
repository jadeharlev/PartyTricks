using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameIconMapping", menuName = "Scriptable Objects/Minigame Icon Mapping")]
public class MinigameIconMappingSO : ScriptableObject {
    [System.Serializable]
    public class IconEntry {
        public MinigameType Type;
        public Texture2D NormalIcon;
        public Texture2D DoubleIcon;
        public Texture2D CompleteIcon;
        public Texture2D CompleteDoubleIcon;
    }

    [SerializeField] private List<IconEntry> icons = new();

    public Texture2D GetIcon(MinigameType type, bool isDouble, bool isComplete = false) {
        var entry = icons.Find(x => x.Type == type);
        if (type == MinigameType.Final) return entry.NormalIcon;
        if (entry == null) {
            Debug.LogError($"Icon missing for type {type}");
            return null;
        }

        if (isComplete) {
            if (isDouble) return entry.CompleteDoubleIcon;
            else return entry.CompleteIcon;
        } 
        
        if (isDouble) {
            return entry.DoubleIcon;
        }
        

        return entry.NormalIcon;
    }
}