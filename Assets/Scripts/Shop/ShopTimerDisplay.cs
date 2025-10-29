using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ShopTimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [FormerlySerializedAs("shopTimer")] [SerializeField] private CountdownTimer CountdownTimer;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private GameObject timerTextGameObject;

    private void Awake() {
        CountdownTimer.OnTick += UpdateTimerText;
        CountdownTimer.OnTimerEnd += OnTimerEnd;
        CountdownTimer.OnReset += Reset;
    }

    private void Reset() {
        labelText.alignment = TextAlignmentOptions.Right;
        timerTextGameObject.SetActive(true);
        labelText.text = "Time Remaining: ";
    }
    private void OnDestroy() {
        CountdownTimer.OnTick -= UpdateTimerText;
        CountdownTimer.OnTimerEnd -= OnTimerEnd;
        CountdownTimer.OnReset -= Reset;
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
