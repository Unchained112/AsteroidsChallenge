using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public Sprite[] asteroid_sprites;
    public float asteroid_size = 1.0f; //{1, 0.5, 0.25} corresponds to 20, 50, 100 scores
    public Vector2 init_speed = new Vector2(1.0f, 0.5f);
    // publish asteroid destoried event
    public delegate void AsteroidDestroied(float size, Vector2 speed, Vector2 pos);
    public event AsteroidDestroied asteroid_die;

    private Rigidbody2D asteroid_body;
    private SpriteRenderer asteroid_renderer;

    private void Awake()
    {
        asteroid_body = GetComponent<Rigidbody2D>();
        asteroid_renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        asteroid_renderer.sprite = asteroid_sprites[Random.Range(0, asteroid_sprites.Length)];
        transform.localScale *= asteroid_size;
        asteroid_body.velocity = init_speed;
    }

    private void Update()
    {
        transform.position = Utilities.WrapPosition(transform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Bullet")
        {
            // suppose bullet mass = 1/64 (around 0.015) large asteroid mass;
            Vector2 v_5 = (0.015f * collision.rigidbody.velocity) + asteroid_body.velocity;
            asteroid_die?.Invoke(asteroid_size, v_5, transform.position);
            Destroy(gameObject, 0.0f);
        }
    }

}
