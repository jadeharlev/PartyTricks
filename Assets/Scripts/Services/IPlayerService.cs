using System;
using System.Collections.Generic;
using Input;
using Player;
using UnityEngine.InputSystem;

namespace Services {
    public interface IPlayerService {
        IReadOnlyList<PlayerSlot> PlayerSlots { get; }
        IEnumerable<PlayerInput> UnassignedInputs { get; }

        bool TryAssignInputToSlot(PlayerInput input, int slotIndex);
        bool TryReleaseSlot(int slotIndex);
        
        public bool TryJoinPlayer(PlayerInput input);
        public void RemovePlayer(int playerIndex);
        
        public void DestroyUnassignedInputs();
        
        PlayerProfile GetPlayerProfile(int playerIndex);
        public bool PlayerIsHuman(int playerIndex);

        public int GetPlayerCount();
        public int GetMaxPlayers();
        public int GetHumanPlayerCount();
        public int GetAIPlayerCount();
        
        public event Action<int, PlayerProfile> OnPlayerJoined;
        public event Action<int> OnPlayerLeft;
        // <playerIndex, newAmount>
        public event Action<int, int> OnPlayerFundsChanged;
        public event Action<PlayerInput> OnInputConnected;
    }
}