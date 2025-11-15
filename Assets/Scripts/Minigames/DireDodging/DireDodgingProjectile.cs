using UnityEngine;

public class DireDodgingProjectile : MonoBehaviour {
    public int OwnerIndex { get; private set; }
    public float Damage { get; private set; }

    private float Speed { get; set; }

    // positive 1: right, negative 1: left
    private int direction;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private SpriteRenderer SpriteRenderer;

    public void Initialize(int ownerIndex, float damage, float speed, Color playerColor, bool movingRight) {
        OwnerIndex = ownerIndex;
        Damage = damage;
        Speed = speed;
        ChangeColor(playerColor);
        if (movingRight) {
            direction = 1;
        }
        else {
            direction = -1;
        }
    }

    private void ChangeColor(Color playerColor) {
        SpriteRenderer.color = playerColor;
    }

    private void Update() {
        var vector3 = transform.position;
        vector3.x = vector3.x + Speed * Time.deltaTime * direction;
        // transform.position = vector3;
        Rigidbody2D.MovePosition(vector3);
    }
}