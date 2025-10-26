using UnityEngine;

[CreateAssetMenu(fileName = "CardDeckSO", menuName = "Scriptable Objects/CardDeck")]
public class CardDeckSO : ScriptableObject {
    public SuitCards ClubCards;
    public SuitCards DiamondCards;
    public SuitCards HeartCards;
    public SuitCards SpadeCards;
}
