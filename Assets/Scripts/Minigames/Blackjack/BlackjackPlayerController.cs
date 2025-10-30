using System;
using UnityEngine;

public class BlackjackPlayerController : MonoBehaviour {
    public int PlayerIndex { get; private set; }
    public bool IsAI { get; private set; }

    private IDirectionalTwoButtonInputHandler navigator;
    private bool inputIsEnabled;

    public event Action<int> OnHit;
    public event Action<int> OnStand;

    public void Initialize(int playerIndex, IDirectionalTwoButtonInputHandler navigator, bool isAI) {
        this.PlayerIndex = playerIndex;
        this.navigator = navigator;
        this.IsAI = isAI;
        inputIsEnabled = false;
    }

    public void EnableInput() {
        inputIsEnabled = true;
    }

    public void DisableInput() {
        inputIsEnabled = false;
    }

    private void Update() {
        if (!inputIsEnabled || IsAI) return;
        if (navigator == null || !navigator.IsActive()) return;

        if (navigator.SelectIsPressed()) {
            OnHit?.Invoke(PlayerIndex);
        }

        if (navigator.CancelIsPressed()) {
            OnStand?.Invoke(PlayerIndex);
        }
}
}