using UnityEngine;

public class PlayerSlot : MonoBehaviour {
    [SerializeField] private int playerIndex;
    public IShopNavigator Navigator { get; private set; }
    public bool IsAI { get; private set; }
    public PlayerProfile Profile { get; private set; }

    public void Initialize(int index, IShopNavigator shopNavigator, bool isAI) {
        playerIndex = index;
        Navigator = shopNavigator;
        IsAI = isAI;
    }

    public void AssignProfile(PlayerProfile profile) {
        this.Profile = profile;
    }

    public void ReplaceShopNavigator(IShopNavigator shopNavigator, bool isAI) {
        this.Navigator = shopNavigator;
        this.IsAI = isAI;
    }
}
