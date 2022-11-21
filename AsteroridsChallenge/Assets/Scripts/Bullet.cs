using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 400.0f;
    public float fly_time = 1.0f;
    private Rigidbody2D bullet_body;
    
    private void Awake()
    {
        bullet_body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        transform.position = Utilities.WrapPosition(transform.position);
    }

    public void BeingShoot(Vector2 dir)
    {
        bullet_body.AddForce(speed * dir);

        Destroy(gameObject, fly_time); // max flying time 1.5s per each bullet
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject, 0.0f);
    }
}
