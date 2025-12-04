using DG.Tweening;
using UnityEngine;

public class DireDodgingHealthBar : MonoBehaviour {
    private Vector3 baseScale;

    private void Awake() {
        this.baseScale = transform.localScale;
    }

    public void UpdateDisplay(float currentHealth, float maxHealth) {
        transform.DOScale(new Vector3(currentHealth / maxHealth, baseScale.y, baseScale.z), 0.25f).SetUpdate(true);
    }
}
