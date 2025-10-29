using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetCard : MonoBehaviour {
    [SerializeField] 
    private Image cardBackground;
    [SerializeField]
    private Sprite betArrowSprite;
    [SerializeField] 
    private Image betArrowUp;
    [SerializeField] 
    private Image betArrowDown;
    [SerializeField] 
    private TMP_Text betText;
    [SerializeField] 
    private Sprite cardUnlockedSprite;
    [SerializeField] 
    private Sprite cardLockedSprite;
    [SerializeField] [Range(0, 3)] [Tooltip("The player number / index")]
    private int playerIndex;

    public void HideDownArrow() {
        HideImage(betArrowDown);
    }
    
    public void HideUpArrow() {
        HideImage(betArrowUp);
    }

    public void SwitchToLockedSprite() {
        cardBackground.sprite = cardLockedSprite;
    }

    public void SwitchToUnlockedSprite() {
        cardBackground.sprite = cardUnlockedSprite;
    }

    public void UpdateBetText(int newValue) {
        betText.text = newValue.ToString();
    }
    
    private void HideImage(Image image) {
        var color = image.color;
        color.a = 0;
        image.color = color;
    }
    
}
