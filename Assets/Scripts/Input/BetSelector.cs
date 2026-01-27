using System;
using UnityEngine;

public class BetSelector : SelectionController
{
    public event Action<BetSelector, int> OnSelectionChanged;

    private float inputTimer;
    private const float INPUT_INTERVAL = 0.10f;
    
    public void Initialize(int index, IDirectionalTwoButtonInputHandler navigator, PlayerProfile profile) {
        PlayerIndex = index;
        Navigator = navigator;
        Profile = profile;
    }
    protected override void HandleNavigation() {
        inputTimer -= Time.deltaTime;
        var direction = Navigator.GetNavigate();
        var discreteDirection = CalculateDiscreteDirection(direction);
        
        if (discreteDirection != Vector2.zero && inputTimer <= 0) {
            inputTimer = INPUT_INTERVAL;
            int delta = BetInputService.GetDelta(discreteDirection);
            OnSelectionChanged?.Invoke(this, delta);
        }
    }
}
