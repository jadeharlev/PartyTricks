using System;
using TMPro;
using UnityEngine;

public class ShopTimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private ShopTimer shopTimer;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private GameObject timerTextGameObject;

    private void Awake() {
        shopTimer.OnTick += UpdateTimerText;
        shopTimer.OnTimerEnd += OnTimerEnd;
        shopTimer.OnReset += Reset;
    }

    private void Reset() {
        labelText.alignment = TextAlignmentOptions.Right;
        timerTextGameObject.SetActive(true);
        labelText.text = "Time Remaining: ";
    }
    private void OnDestroy() {
        shopTimer.OnTick -= UpdateTimerText;
        shopTimer.OnTimerEnd -= OnTimerEnd;
        shopTimer.OnReset -= Reset;
    }

    private void UpdateTimerText(int timeRemaining) {
        timerText.text = timeRemaining + " seconds";
    }

    private void OnTimerEnd() {
        labelText.text = "Time's up!";
        labelText.alignment = TextAlignmentOptions.Center;
        timerTextGameObject.SetActive(false);
    }
}
