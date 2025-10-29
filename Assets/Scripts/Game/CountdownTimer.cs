using System;
using System.Collections;
using UnityEngine;

public class CountdownTimer : MonoBehaviour {
    private int countdownDurationInSeconds;
    private int timeRemaining;
    public Action OnTimerEnd;
    public Action<int> OnTick;
    public Action OnReset;

    public void StartTimer(int ShopDurationInSeconds) {
        this.countdownDurationInSeconds = ShopDurationInSeconds;
        StartCoroutine(Countdown());
    }
    
    private IEnumerator Countdown() {
        timeRemaining = countdownDurationInSeconds;
        while (timeRemaining > 0) {
            OnTick?.Invoke(timeRemaining);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        OnTimerEnd?.Invoke();
    }

    public void Reset() {
        StopAllCoroutines();
        OnReset?.Invoke();
    }
}
