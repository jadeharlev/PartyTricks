using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Services {
    public interface IPlayerService {
        IReadOnlyList<PlayerSlot> PlayerSlots { get; }

        public bool TryJoinPlayer(PlayerInput input);
        public void RemovePlayer(int playerIndex);
        
        PlayerProfile GetPlayerProfile(int playerIndex);
        public bool PlayerIsHuman(int playerIndex);
        public bool SlotIsOccupied(int slotIndex);

        public int GetPlayerCount();
        public int GetHumanPlayerCount();
        public int GetAIPlayerCount();
        
        public event System.Action<int, PlayerProfile> OnPlayerJoined;
        public event System.Action<int> OnPlayerLeft;
        // <playerIndex, newAmount>
        public event System.Action<int, int> OnPlayerFundsChanged;
    }
}