using UnityEngine;
using VineSwinging.Core;

[CreateAssetMenu(fileName = "VineSwingingPlayerStatsSO", menuName = "Scriptable Objects/VineSwingingPlayerStatsSO")]
public class VineSwingingPlayerStatsSO : ScriptableObject {
    [Header("Swing Settings")] 
    [SerializeField] public float Amplitude = 1.4f;
    [SerializeField] public float RopeLength = 4f;
    [SerializeField] public float Period = 3.5f;
    [SerializeField] public float LaunchForce = 1.3f;
    [SerializeField] public float GrabRadius = 2.5f;
    [SerializeField] public float VineSpacing = 10f;
    [SerializeField] public float Gravity = 15f;
    [SerializeField] [Range(0, 0.3f)] [Tooltip("How much variation should be present in vine swing positions?")]
    public float PeriodVariation = 0.1f;
    

    [Header("Fall/Respawn")] 
    [SerializeField] public float FallThresholdY = -8f;
    [SerializeField] public float RespawnDelayInSeconds = 1f;

    public SwingConfig CreateConfig() {
        return new SwingConfig(Amplitude, RopeLength, Period, LaunchForce, GrabRadius, FallThresholdY, RespawnDelayInSeconds, VineSpacing, Gravity);
    }
}
