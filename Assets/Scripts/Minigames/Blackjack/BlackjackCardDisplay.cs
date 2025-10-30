using UnityEngine;

public class BlackjackCardDisplay : MonoBehaviour {
    [SerializeField]
    private GameObject PlayingCardPrefab;

    public void AddCard(Card card) {
        GameObject newCard = Instantiate(PlayingCardPrefab);
        newCard.transform.SetParent(transform);
        newCard.GetComponent<PlayingCard>().SetCard(card);
        newCard.transform.localScale = Vector3.one;
        newCard.transform.localRotation = Quaternion.identity;
        newCard.transform.localPosition = Vector3.zero;
    }
}
