using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MinigameStartCountdown : MonoBehaviour
{
    [FormerlySerializedAs("countdownText")] [SerializeField] private TMP_Text CountdownText;
    public event Action OnTimerEnd;
    private int timeRemaining;

    public void Initialize(int numberOfSeconds) {
        DebugLogger.Log(LogChannel.Systems, "Initialized countdown timer", LogLevel.Verbose);
        timeRemaining = numberOfSeconds;
        CountdownText.text = numberOfSeconds.ToString();
    }

    public void StartTimer() {
        DebugLogger.Log(LogChannel.Systems, "Started countdown timer");
        StartCoroutine(Timer());
    }

    private IEnumerator Timer() {
        while (timeRemaining > 0) {
            OnTick(timeRemaining);
            timeRemaining--;
            DebugLogger.Log(LogChannel.Systems, "Countdown timer ticked: " + timeRemaining  + " seconds remaining.");
            yield return new WaitForSeconds(1f);
        }
        OnTimerEnd?.Invoke();
        Destroy(gameObject);
    }

    private void OnTick(int timeRemaining) {
        CountdownText.text = timeRemaining.ToString();
    }
}
