using UnityEngine;

public class BlackjackControlsDisplay : MonoBehaviour {
    [SerializeField] 
    private BlackjackControlDisplay HitControl;
    [SerializeField] 
    private BlackjackControlDisplay StandControl;

    public void ShowControls() {
        HitControl.Show();
        StandControl.Show();
    }
    
    public void HideControls() {
        HitControl.Hide();
        StandControl.Hide();
    }
}
