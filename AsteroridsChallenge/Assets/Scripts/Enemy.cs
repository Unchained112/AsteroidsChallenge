using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Bullet bullet_prefab;
    public Player player;
    public AudioSource sound_shoot_prefab;
    public float speed = 1.0f;
    // publish enemy destoried event
    public delegate void EnemyDestoried(Vector2 pos);
    public event EnemyDestoried enemy_die;

    private AudioSource sound_shoot;
    private Rigidbody2D enemy_body;
    private Vector3 dir = new Vector3(1.0f, 0.0f, 0.0f);

    private void Awake()
    {
        sound_shoot = Instantiate(sound_shoot_prefab);
        enemy_body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        dir = new Vector3(0.0f, 0.0f, 0.0f) - transform.position;
        dir.Normalize();
        enemy_body.velocity = dir * speed;
        StartCoroutine(Shoot());
    }

    private void Update()
    {
        transform.position = Utilities.WrapPosition(transform.position);
    }

    private void FixedUpdate()
    {
        //enemy_body.MovePosition(transform.position + (dir * speed * Time.deltaTime));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            enemy_die?.Invoke(transform.position);
            Destroy(gameObject, 0.0f);
        }
    }

    private IEnumerator Shoot()
    {
        while (true && player != null)
        {
            yield return new WaitForSeconds(3.0f); 
            sound_shoot.Play();
            Bullet bullet = Instantiate(bullet_prefab, transform.position, transform.rotation);
            dir = player.transform.position - transform.position;
            dir.Normalize();
            bullet.BeingShoot(dir);
            yield return new WaitForSeconds(2.0f);
            enemy_body.velocity = dir * speed;
        }
    }
}
