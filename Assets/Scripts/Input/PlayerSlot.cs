using UnityEngine;

public class PlayerSlot : MonoBehaviour {
    [SerializeField] private int playerIndex;
    public IDirectionalTwoButtonInputHandler Navigator { get; private set; }
    public bool IsAI { get; private set; }
    public PlayerProfile Profile { get; private set; }

    public void Initialize(int index, IDirectionalTwoButtonInputHandler directionalTwoButtonInputHandler, bool isAI) {
        playerIndex = index;
        Navigator = directionalTwoButtonInputHandler;
        IsAI = isAI;
    }

    public void AssignProfile(PlayerProfile profile) {
        this.Profile = profile;
    }

    public void ReplaceDirectionalTwoButtonInputHandler(IDirectionalTwoButtonInputHandler directionalTwoButtonInputHandler, bool isAI) {
        this.Navigator = directionalTwoButtonInputHandler;
        this.IsAI = isAI;
    }
}
