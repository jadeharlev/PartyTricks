using System;
using System.Collections;
using UnityEngine;

public class ShopTimer : MonoBehaviour {
    private int shopDurationInSeconds;
    private int timeRemaining;
    public Action OnTimerEnd;
    public Action<int> OnTick;
    public Action OnReset;

    public void StartTimer(int ShopDurationInSeconds) {
        this.shopDurationInSeconds = ShopDurationInSeconds;
        StartCoroutine(ShopCountdown());
    }
    
    private IEnumerator ShopCountdown() {
        timeRemaining = shopDurationInSeconds;
        while (timeRemaining > 0) {
            Debug.Log("Shop time remaining: " + timeRemaining + " seconds");
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
