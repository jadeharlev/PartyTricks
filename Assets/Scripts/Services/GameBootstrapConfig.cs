using UnityEngine;

[CreateAssetMenu(fileName = "GameBootstrapConfig", menuName = "Scriptable Objects/GameBootstrapConfig")]
public class GameBootstrapConfig : ScriptableObject {
    public GameObject PlayerServicePrefab;
    public GameObject GameSessionManagerPrefab;
    public GameObject PauseServicePrefab;
    public GameObject GameFlowManagerPrefab;
    public GameObject EconomyServicePrefab;

    // Where is this file stored? (within the Resources/ folder)
    public static readonly string ResourcePath = "Config/GameBootstrapConfig";
}
