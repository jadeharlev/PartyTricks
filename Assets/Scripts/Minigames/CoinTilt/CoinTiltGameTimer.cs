using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoinTiltGameTimer : MonoBehaviour {
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private TMP_Text TimerText;
    public event Action OnTimerEnd;
    private int RemainingTimeInSeconds { get; set; }

    public void Initialize(int gameLengthInSeconds) {
        RemainingTimeInSeconds = gameLengthInSeconds;
        HidePanel();
    }

    private void ShowPanel() {
        TimerPanel.SetActive(true);
    }

    private void HidePanel() {
        TimerPanel.SetActive(false);
    }

    public void StartTimer() {
        ShowPanel();
        StartCoroutine(Timer());
    }

    private IEnumerator Timer() {
        while (RemainingTimeInSeconds > 0) {
            OnTick(RemainingTimeInSeconds);
            RemainingTimeInSeconds--;
            DebugLogger.Log(LogChannel.Systems, "Game timer ticked: " + RemainingTimeInSeconds  + " seconds remaining.");
            yield return new WaitForSeconds(1f);
        }
        OnTimeUp();
        OnTimerEnd?.Invoke();
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
}
