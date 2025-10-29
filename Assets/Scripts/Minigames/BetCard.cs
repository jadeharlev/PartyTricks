using System.Collections;
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

    private Coroutine downArrowFlashRoutine;
    private Coroutine upArrowFlashRoutine;

    public void ShowUpArrow() {
        ShowImage(betArrowUp);
    }
    
    public void ShowDownArrow() {
        ShowImage(betArrowDown);
    }

    public void HideDownArrow() {
        if (downArrowFlashRoutine != null) {
            StopCoroutine(downArrowFlashRoutine);
            downArrowFlashRoutine = null;
        }
        HideImage(betArrowDown);
    }
    
    public void HideUpArrow() {
        if (upArrowFlashRoutine != null) {
            StopCoroutine(upArrowFlashRoutine);
            upArrowFlashRoutine = null;
        }
        HideImage(betArrowUp);
    }

    public void SwitchToLockedSprite() {
        cardBackground.sprite = cardLockedSprite;
    }

    public void SwitchToUnlockedSprite() {
        cardBackground.sprite = cardUnlockedSprite;
    }

    public void FlashArrow(bool betIsAnIncrease) {
        if (betIsAnIncrease) {
            if (upArrowFlashRoutine != null) StopCoroutine(upArrowFlashRoutine);
            upArrowFlashRoutine = StartCoroutine(FlashArrowRoutine(betIsAnIncrease));
        }
        else {
            if (downArrowFlashRoutine != null) StopCoroutine(downArrowFlashRoutine);
            downArrowFlashRoutine = StartCoroutine(FlashArrowRoutine(betIsAnIncrease));
        }
    }

    private IEnumerator FlashArrowRoutine(bool betIsAnIncrease) {
        Image arrow = betIsAnIncrease ? betArrowUp : betArrowDown;
        float currentAlpha = arrow.color.a;
        if (currentAlpha > 0) {
            Color flashColor = Color.yellow;
            flashColor.a = currentAlpha;
            arrow.color = flashColor;
            
            yield return new WaitForSeconds(0.15f);
            
            Color originalColor = Color.white;
            arrow.color = Color.yellow;
            arrow.color = originalColor;
        }
        if (betIsAnIncrease) {
            upArrowFlashRoutine = null;
        } else {
            downArrowFlashRoutine = null;
        }
    }

    public void UpdateBetText(int newValue) {
        betText.text = newValue.ToString();
    }
    
    private void HideImage(Image image) {
        var color = image.color;
        color.a = 0;
        image.color = color;
    }
    
    private void ShowImage(Image image) {
        image.color = Color.white;
    }
}
