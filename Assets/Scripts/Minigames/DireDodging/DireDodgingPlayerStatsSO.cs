using UnityEngine;

[CreateAssetMenu(fileName = "DireDodgingPlayerStatsSO", menuName = "Scriptable Objects/DireDodgingPlayerStatsSO")]
public class DireDodgingPlayerStatsSO : ScriptableObject {
    [Header("Movement Settings")] 
    public float MoveSpeed = 15f;
    public float ProjectileScale = 1f;
    public float ProjectileSpeed = 15f;
    public float ProjectileShootRate = 1f;
    public float BaseDamage = 1f;
    public float BaseHealth = 15f;
}
