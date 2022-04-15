using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioSource musicSource;

    public AudioClip throwCog;
    public AudioClip playerHit;
    public AudioClip winMusic;
    public AudioClip loseMusic;
    public AudioClip backgroundMusic;

    private bool musicToggle = false;

    public float speed = 3.0f;
    public float timeInvincible = 2.0f;

    bool isInvincible;
    float invincibleTimer;

    public int maxHealth = 5;
    public int health { get { return currentHealth; } }
    int currentHealth;

    public int score;
    public Text scoreText;

    public int cogs = 4;
    public Text cogsText;

    public Text gameOverText;

    public GameObject projectilePrefab;
    public ParticleSystem damageEffect;

    float horizontal;
    float vertical;

    public bool gameOver = false;
    public static int level = 1;

    Rigidbody2D rigidbody2d;
    Animator animator;

    Vector2 lookDirection = new Vector2(1, 0);

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        cogs = 4;

        scoreText.text = "Robots Fixed: " + score.ToString();
        cogsText.text = "Cogs: " + cogs.ToString();

        gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (health == 0)
        {
            gameOverText.text = "You Lost! Press R to Restart. Game Created by Giovanni Corchado";
            gameOverText.gameObject.SetActive(true);
            gameOver = true;
            speed = 0.0f;

            if (musicToggle == false)
            {
                musicToggle = true;
                musicSource.clip = loseMusic;
                musicSource.Play();
            }
        }

        if (score == 4 && level == 1)
        {
            gameOverText.text = "Talk to Jambi to visit stage two!";
            gameOverText.gameObject.SetActive(true);
        }
        else if (score == 4 && level == 2)
        {
            gameOverText.text = "You Won! Press R to Restart. Game Created by Giovanni Corchado";
            gameOverText.gameObject.SetActive(true);
            speed = 0.0f;

            if (musicToggle == false)
            {
                musicToggle = true;
                musicSource.clip = winMusic;
                musicSource.Play();
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (score == 4 && level == 2)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (score == 4)
                {
                    level = 2;
                    SceneManager.LoadScene("Main 1");
                }
                else
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        character.DisplayDialog();
                    }
                }
            }
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    void FixedUpdate()
    {
        Vector2 position = transform.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;
        transform.position = position;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");

            if (isInvincible)
                return;

            isInvincible = true;
            Instantiate(damageEffect, transform.position, Quaternion.identity);
            invincibleTimer = timeInvincible;
            audioSource.PlayOneShot(playerHit);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    public void ChangeScore()
    {
        score += 1;
        scoreText.text = "Robots Fixed: " + score.ToString();
    }

    public void ChangeAmmo()
    {
        cogs += 4;
        cogsText.text = "Cogs: " + cogs.ToString();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void Launch()
    {
        if (cogs >= 1)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);
            audioSource.PlayOneShot(throwCog);

            cogs -= 1;
            cogsText.text = "Cogs: " + cogs.ToString();

            animator.SetTrigger("Launch");
        }
    }
}
