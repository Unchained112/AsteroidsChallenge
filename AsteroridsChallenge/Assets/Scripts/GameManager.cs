using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject  UI_title;
    public GameObject UI_start_button;
    public GameObject UI_score;
    public GameObject UI_quit_button;
    public GameObject UI_restart_button;
    public GameObject UI_game_over;
    public AudioSource sound_click;
    public AudioSource sound_explosion;
    public AudioSource sound_enemy_1;
    public AudioSource sound_enemy_2;
    public AudioSource sound_lfie;
    public Image[] life_count;
    public Player player_prefab;
    public Asteroid asteroid_prefab;
    public Enemy enemy_prefab;
    public ParticleSystem ast_explosion;
    public ParticleSystem flight_explosion;

    private Button start_button;
    private Button restart_button;
    private Button quit_button;
    private TextMeshProUGUI total_score;
    private Player player;
    private Queue<Asteroid> asteroids;
    private Queue<Enemy> enemies;
    private int player_lifes = 3;
    private int score = 0; // asteroid score (Large: 20; Mid: 50; Small: 100)
    private int num_of_asts = 4; // begin with 4 asteroids
    private int num_asts_sum = 4; // total killed number
    private int add_life = 0; // store score to check whether to add life
    private int bonus = 0; // score get by killing enemies
    private int total = 0; // total score, update every frame
    private Vector2 boundary;

    // ------------- Game Manager ----------- //

    private void Start()
    {
        start_button = UI_start_button.GetComponent<Button>();
        start_button.onClick.AddListener(ClickStartButton);
        restart_button = UI_restart_button.GetComponent<Button>();
        restart_button.onClick.AddListener(ClickStartButton);
        quit_button = UI_quit_button.GetComponent<Button>();
        quit_button.onClick.AddListener(ClickQuitButton);
        total_score = UI_score.GetComponent<TextMeshProUGUI>();
        boundary = Utilities.GetBoundary();
    }

    private void Update()
    {
        // 1 * 20 + 2 * 50 + 4 * 100 = 520
        if(score > 0 && score % (520 * num_asts_sum) == 0 && score <= 40000) // level up
        {
            num_of_asts += 2;
            num_asts_sum += num_of_asts;
            foreach (Asteroid asteroid in asteroids)
            {
                asteroid.asteroid_die -= OnAsteroidDie; // unsubscribe ast
            }
            asteroids.Clear();
            SpawnAsteroid();
        }
        if(total >= 10000 && (total - add_life) >= 10000) // add one life every 10000 score, max 5
        {
            add_life = total;
            player_lifes = Mathf.Min(5, player_lifes + 1);
            life_count[player_lifes - 1].enabled = true;
            sound_lfie.Play();
        }
        total = score + bonus;
        total_score.text = "Score: " + total.ToString();
    }

    private void ClickStartButton()
    {
        UI_title.SetActive(false);
        UI_game_over.SetActive(false);
        UI_start_button.SetActive(false);
        UI_restart_button.SetActive(false);
        UI_quit_button.SetActive(false);
        UI_score.SetActive(true);
        sound_click.Play();
        StartGame();
    }

    private void ClickQuitButton()
    {
        StartCoroutine(CloseGame());
    }

    private void StartGame()
    {
        score = 0;
        num_of_asts = 4; 
        num_asts_sum = 4; 
        player_lifes = 3;
        add_life = 0;
        total = 0;
        bonus = 0;
        player = Instantiate(player_prefab, new Vector2(0.0f, 0.0f), Quaternion.identity);
        player.on_flight_die += OnFlightDie; // subscribe to the flight die event
        asteroids = new Queue<Asteroid>();
        enemies = new Queue<Enemy>();
        SpawnAsteroid();
        for(int i = 0; i < player_lifes; i++) { life_count[i].enabled = true; } // display lives
        StartCoroutine(SpawnEnemy());
    }

    private void GameOver()
    {
        Destroy(player);
        StopAllCoroutines();
        foreach (Asteroid asteroid in asteroids)
        {
            if (asteroid != null) Destroy(asteroid.gameObject, 0.0f);
            asteroid.asteroid_die -= OnAsteroidDie; // unsubscribe ast
        }
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null) Destroy(enemy.gameObject, 0.0f);
            enemy.enemy_die -= OnEnemyDie; // unsubscribe ast
        }
        asteroids.Clear();
        enemies.Clear();
        UI_game_over.SetActive(true);
        UI_restart_button.SetActive(true);
        UI_quit_button.SetActive(true);
    }

    private IEnumerator CloseGame()
    {
        sound_click.Play();
        yield return new WaitForSeconds(0.5f);
        // UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    // ------------- Asteroid ---------------- //

    private void OnAsteroidDie(float size, Vector2 speed, Vector2 pos)
    {
        switch (size)
        {
            case 1.0f:
                score += 20;
                break;
            case 0.5f:
                score += 50;
                break;
            case 0.25f:
                score += 100;
                break;
            default:
                break;
        }
        ast_explosion.transform.position = pos;
        ast_explosion.Play();
        sound_explosion.Play();

        if(size > 0.25f)
        {
            Vector2 v_3 = new Vector2(
                (0.71f * speed.x) - (0.71f * speed.y),
                (0.71f * speed.x) + (0.71f * speed.y));
            CreateAsteroid(0.5f * size, v_3, pos);

            Vector2 v_4 = new Vector2(
                (0.71f * speed.x) - (-0.71f * speed.y),
                (-0.71f * speed.x) + (0.71f * speed.y));
            CreateAsteroid(0.5f * size, v_4, pos);
        }
    }

    private void CreateAsteroid(float size, Vector2 speed, Vector2 pos)
    {
        Asteroid ast = Instantiate(asteroid_prefab, pos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360)));
        ast.init_speed = speed * 0.6f;
        ast.asteroid_size = size;
        ast.asteroid_die += OnAsteroidDie; // subcribe to the asteroid when spwaning
        asteroids.Enqueue(ast);
    }

    private void SpawnAsteroid()
    {
        for (int i = 0; i < num_of_asts; i++)
        {
            Vector2 pos;
            Vector2 sp;
            switch (i % 4)
            {
                case 0:
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                        pos = new Vector2(boundary.x, Random.Range(0.0f, boundary.y));
                    else
                        pos = new Vector2(Random.Range(0.0f, boundary.x), boundary.y);
                    sp = new Vector2(0.0f, -boundary.y) - pos;
                    sp.Normalize();
                    CreateAsteroid(1.0f, sp, pos);
                    break;
                case 1:
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                        pos = new Vector2(boundary.x, Random.Range(0.0f, -boundary.y));
                    else
                        pos = new Vector2(Random.Range(0.0f, boundary.x), -boundary.y);
                    sp = new Vector2(-boundary.x, 0.0f) - pos;
                    sp.Normalize();
                    CreateAsteroid(1.0f, sp, pos);
                    break;
                case 2:
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                        pos = new Vector2(-boundary.x, Random.Range(0.0f, -boundary.y));
                    else
                        pos = new Vector2(Random.Range(0.0f, -boundary.x), -boundary.y);
                    sp = new Vector2(0.0f, boundary.y) - pos;
                    sp.Normalize();
                    CreateAsteroid(1.0f, sp, pos);
                    break;
                case 3:
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                        pos = new Vector2(-boundary.x, Random.Range(0.0f, boundary.y));
                    else
                        pos = new Vector2(Random.Range(0.0f, -boundary.x), boundary.y);
                    sp = new Vector2(boundary.x, 0.0f) - pos;
                    sp.Normalize();
                    CreateAsteroid(1.0f, sp, pos);
                    break;
                default:
                    break;
            }
        }
    }

    // ----------- Player (Flight) ---------- //

    private void OnFlightDie(object sender, System.EventArgs e)
    {
        sound_explosion.Play();
        player_lifes--;
        flight_explosion.transform.position = player.transform.position;
        flight_explosion.Play();
        life_count[player_lifes].enabled = false;
        if (player_lifes <= 0)
        {
            player.on_flight_die -= OnFlightDie; // unsubscribe player
            GameOver();
        }
        else
        {
            StartCoroutine(FlightRespawn()); // respawn
        }
    }

    private IEnumerator FlightRespawn()
    {
        yield return new WaitForSeconds(2.0f);
        if(player != null)
        {
            player.gameObject.SetActive(true);
            player.transform.position = new Vector2(0.0f, 0.0f);
            player.transform.rotation = Quaternion.identity;
        }
    }

    // --------------- Enemy --------------- //

    private IEnumerator SpawnEnemy() {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(2.0f, 40.0f - total * 0.0009f));
            Vector2 pos;
            if (Random.Range(0.0f, 1.0f) > 0.5f) pos = new Vector2(-boundary.x + 1.0f, 2.0f);
            else pos = new Vector2(boundary.x - 1.0f, -2.0f);
            Enemy emy = Instantiate(enemy_prefab, pos, Quaternion.identity);
            emy.player = player;
            emy.enemy_die += OnEnemyDie;
            enemies.Enqueue(emy);
            sound_enemy_1.Play();
            yield return new WaitForSeconds(sound_enemy_1.clip.length);
            sound_enemy_1.Play();
            yield return new WaitForSeconds(sound_enemy_1.clip.length);
            sound_enemy_2.Play();
            yield return new WaitForSeconds(sound_enemy_2.clip.length);
            sound_enemy_2.Play();
            yield return new WaitForSeconds(sound_enemy_2.clip.length);
        }
    }

    private void OnEnemyDie(Vector2 pos)
    {
        ast_explosion.transform.position = pos;
        ast_explosion.Play();
        sound_explosion.Play();
        bonus += 100;
    }
}
