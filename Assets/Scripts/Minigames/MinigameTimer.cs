using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MinigameTimer : MonoBehaviour {
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private TMP_Text TimerText;
    public event Action OnTimerEnd;
    public event Action<int> OnHalfwayPointReached;
    private string endOfGameText;
    private int RemainingTimeInSeconds { get; set; }
    private int originalTimerDuration;
    private Coroutine timerCoroutine = null;

    public void Initialize(int gameLengthInSeconds, string endOfGameText = "Game!") {
        originalTimerDuration = gameLengthInSeconds;
        RemainingTimeInSeconds = gameLengthInSeconds;
        this.endOfGameText = endOfGameText;
        HidePanel();
    }

    public void OverrideText(string text) {
        TimerText.text = text;
    }

    private void ShowPanel() {
        TimerPanel.SetActive(true);
    }

    private void HidePanel() {
        TimerPanel.SetActive(false);
    }

    public void StartTimer() {
        ShowPanel();
        timerCoroutine = StartCoroutine(Timer());
    }

    private IEnumerator Timer() {
        while (RemainingTimeInSeconds > 0) {
            OnTick(RemainingTimeInSeconds);
            RemainingTimeInSeconds--;
            DebugLogger.Log(LogChannel.Systems, "Game timer ticked: " + RemainingTimeInSeconds  + " seconds remaining.");
            if (RemainingTimeInSeconds == (originalTimerDuration / 2)) {
                OnHalfwayPointReached?.Invoke(RemainingTimeInSeconds);
            }
            yield return new WaitForSeconds(1f);
        }
        OnTimeUp();
        OnTimerEnd?.Invoke();
        timerCoroutine = null;
    }

    private void OnTimeUp() {
        TimerText.text = "Time!";
    }

    private void OnTick(int remainingTimeInSeconds) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTimeInSeconds);
        string timeInMinutes = timeSpan.Minutes.ToString("00");
        string timeInSeconds = timeSpan.Seconds.ToString("00");
        TimerText.text = timeInMinutes + ":" + timeInSeconds;
    }

    public void StopIfRunning() {
        if (timerCoroutine != null) {
            StopCoroutine(this.timerCoroutine);
            if (!string.IsNullOrEmpty(endOfGameText)) {
                TimerText.text = endOfGameText;
            }
            timerCoroutine = null;
        }
    }
}
