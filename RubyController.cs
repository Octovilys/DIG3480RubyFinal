using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;



public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;
    private float boostTimer;
    private bool boosting;
   
    public int maxHealth = 5;
    
    public GameObject projectilePrefab;


    public int Score;
   
    int Cog;
    public int Book;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI CogText;
    public TextMeshProUGUI BookText;


    public GameObject winTextObject;
    public GameObject loseTextObject;
    public GameObject LevelOneTextObject;


    public GameObject BackgroundMusicObject;
    public GameObject WinSoundObject;
    public GameObject LoseSoundObject;

    
    private static int Level = 1;
   
    
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip collectedClip;
    public AudioClip splashSound;
    public AudioClip reloadSound;
    public AudioClip pageSound;
   
    
    public int health { get { return currentHealth; }}
    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;
    
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    
    public ParticleSystem HealthIncrease;
    public ParticleSystem HealthDecrease;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    
    AudioSource audioSource;

    public bool gameOver = false;
    public static bool staticVar = false;

    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();

        Score = 0;
        Cog = 4;
        Book = 0;

        SetCogText();
        SetBookText();

        winTextObject.SetActive(false);
        loseTextObject.SetActive(false);
        LevelOneTextObject.SetActive(false);

        WinSoundObject.SetActive(false);
        LoseSoundObject.SetActive(false);


        HealthIncrease.Stop();
        HealthDecrease.Stop();

        speed = 3;
        boostTimer = 0;
        boosting = false;

    }

    public void ChangeScore(int scoreAmount)
    {
        {   
            Score++;
            scoreText.text = "Fixed Robots: " + Score.ToString();
        }

       if (Level == 1)
        {
          if (Score == 4)
            {
                LevelOneTextObject.SetActive(true);
            }  
        }

        if (Level == 2)
        {
          if(Score == 4 && Book == 4)
            {
                winTextObject.SetActive(true);
                speed = 0;
                WinSoundObject.SetActive(true);
                BackgroundMusicObject.SetActive(false);
                gameOver = true; 
            }
        }

    }


    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        Vector2 move = new Vector2(horizontal, vertical);
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
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
        
        if(Input.GetKeyDown(KeyCode.C) && Cog >= 1)
        {
           Cog = Cog -1;
           SetCogText();
            Launch();

        }
        
        else if (Input.GetKeyDown(KeyCode.C) && Cog ==0)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (Score >= 4)
                {
                    Level = 2;
                    SceneManager.LoadScene(1);
                    Score = 4;
                }

                else if (Score < 4)
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)

                    {
                        character.DisplayDialog();
                    }
                }

                if (Book < 4)
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        character.DisplayDialog();
                    }
                }
            }
        }

        if(boosting)
        {
            boostTimer += Time.deltaTime;
            if(boostTimer >= 5)
            {
                speed = 3;
                boostTimer = 0;
                boosting = false;
            }
        }


        if (currentHealth == 0) 
        {
            loseTextObject.SetActive(true);
            {
                speed = 0;
                LoseSoundObject.SetActive(true);
                BackgroundMusicObject.SetActive(false);
                gameOver = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            if (gameOver == true)
            { 
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    
    void SetCogText()
    {
     if (Cog > 0)
        {
			CogText.text = "Cogs: " + Cog.ToString();
	    }

        else if (Cog <= 0)
        {
			CogText.text = "Out of Cogs!";
        }
    }

    void SetBookText()
    {
        if (Book > 0)
        {
            BookText.text = "Books: " +  Book.ToString();
        }
    }


    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Cog"))
        {
            Cog = Cog + 3;
            PlaySound(reloadSound);
            other.gameObject.SetActive(false);
            SetCogText();
        
        }

        if(other.tag == "SpeedBoost")
        {
            boosting = true;
            speed = 6;
            PlaySound(splashSound);
            Destroy(other.gameObject);
        }

        if(other.tag == "Book")
        {
            Book = Book + 1;
            PlaySound(pageSound);
            other.gameObject.SetActive(false);
            SetBookText();
        }
    }


  public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            GameObject projectileObject = Instantiate(HealthDecrease.gameObject, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            PlaySound(hitSound);
        }

        if (amount < 5)
        {
            GameObject projectileObject = Instantiate(HealthIncrease.gameObject, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }
        

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }
    
    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");
        
        PlaySound(throwSound);

    } 

   public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}


    