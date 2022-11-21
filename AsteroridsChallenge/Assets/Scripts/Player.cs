using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Sprite[] flight_sprites;
    public Bullet bullet_prefab;
    public event EventHandler on_flight_die; // publish flight die event
    public AudioSource sound_shoot_prefab;

    // Flight properties
    private float flight_speed = 5.0f;
    private float turn_speed = 0.5f;
    private bool flying;
    private float turning;
    private Rigidbody2D flight_body;
    private SpriteRenderer flight_renderer;
    private AudioSource sound_shoot;
    

    private void Awake()
    {
        flight_body = GetComponent<Rigidbody2D>();
        flight_renderer = GetComponent<SpriteRenderer>();
        sound_shoot = Instantiate(sound_shoot_prefab);
    }

    private void Update()
    {
        // flght with fire
        if (flying)
        {
            flight_renderer.sprite = flight_sprites[1]; // change to animation later
        }
        else
        {
            flight_renderer.sprite = flight_sprites[0];
        }

        // Contorl - movement
        flying = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            turning = 1.0f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            turning = -1.0f;
        }
        else
        {
            turning = 0.0f;
        }

        // Control - shooting
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        // Check flight position
        transform.position = Utilities.WrapPosition(transform.position);
        
    }

    private void FixedUpdate()
    {
        if (flying)
        {
            flight_body.AddForce(transform.up * flight_speed);
        }

        if (turning != 0.0f)
        {
            flight_body.AddTorque(turning * turn_speed);
        }
    }

    private void Shoot()
    {
        sound_shoot.Play();
        Bullet bullet = Instantiate(bullet_prefab, transform.position, transform.rotation);
        bullet.BeingShoot(transform.up);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Asteroid" || collision.gameObject.tag == "Enemy")
        {
            gameObject.SetActive(false);
            on_flight_die?.Invoke(this, EventArgs.Empty);
        }
    }
}
