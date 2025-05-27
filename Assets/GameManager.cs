using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 0;

    public GameObject GameOverScreen, GameWinScreen, InfoScreen;
    public GameObject  Information;
    public bool GameState = false;
    public BasePlayer Player;

    private ScoreObj Score;

  
    private Vector2 tapPosition;
    public ParticleSystem poff;
    private List<GameObject> spawnedFaces = new List<GameObject>();

    

    public Text ScoreText;
    private int currentScore;

    public AudioSource GameOverReal;
    public AudioSource GameOverCartoon;
    public AudioSource Coin;
    public AudioSource Tap;
    public AudioSource UISound;

    public GameObject[] backgrounds;


    private float lastTapTime = 0f; // Time since the last tap
    private float tapDecayDelay = 0f;

    private int currentIndex = 0;
    public List<GameObject> aldoChildren = new List<GameObject>();

    public UnityEngine.UI.Slider progressBar;
    public float decayRate = 0.3f;

    public int tappedAldoCount = 0;

    public ParticleSystem poof;
    public GameObject[] hearts;
    [HideInInspector] public int collisionCount = 0;

    public float baseScore = 10f;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start()
    {
        InfoScreen.SetActive(true);
        // StartCoroutine(SpawnFacesCoroutine());
        ActivateBackground(currentIndex);

    }

    void Update()
    {
        if (!GameState)
            return;

        if (progressBar.value == 0)
        {
            GameOVer();
        }

        if (Time.time - lastTapTime > tapDecayDelay)
        {
            DecreaseSliderValue();

        }



    }

    private void DecreaseSliderValue()
    {
        float previousValue = progressBar.value;
        if (progressBar != null && progressBar.value > 0)
        {
            progressBar.value -= decayRate * Time.deltaTime; // Decrease over time

            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);
        }
    }

    public void PlayGame()
    {
        GameState = true;
        Time.timeScale = 1;
        Information.SetActive(false);
    }
    public void PauseGame()
    {
        GameState = false;
        Information.SetActive(true);
        StartCoroutine(Pause());
    }
    public void uiSound()
    {
        UISound.Play();
    }

    IEnumerator Pause()
    {
 

        // Wait for a specified duration (adjust the delay as needed)
        yield return new WaitForSeconds(0.2f);
        Time.timeScale = 0;


    }


   
    public void ActivateBackground(int index)
    {
        // Check if the index is within range.
        if (index < 0 || index >= backgrounds.Length)
        {
            Debug.LogWarning("Index out of range!");
            return;
        }

        // Loop through all backgrounds and activate only the one at the given index.
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].SetActive(i == index);
        }

        // Update the current index.
        currentIndex = index;

        GetAldoChildObjects();
    }

    // Cycles to the next background in the array.
    public void ActivateNextBackground()
    {
        aldoChildren.Clear();
        // Increment the index and loop back to 0 if needed.
        currentIndex = (currentIndex + 1) % backgrounds.Length;
        ActivateBackground(currentIndex);
    }

    public List<GameObject> GetAldoChildObjects()
    {
        // Clear the list to remove any previous entries.
        aldoChildren.Clear();

        if (backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogWarning("Backgrounds array is empty.");
            return aldoChildren;
        }
        if (currentIndex < 0 || currentIndex >= backgrounds.Length)
        {
            Debug.LogWarning("Current index is out of range.");
            return aldoChildren;
        }

        GameObject currentBackground = backgrounds[currentIndex];

        // Loop through all child transforms of the current background and collect those with the "aldo" tag.
        foreach (Transform child in currentBackground.transform)
        {
            if (child.CompareTag("aldo"))
            {
                aldoChildren.Add(child.gameObject);
            }
        }

        // First, deactivate all child objects
        foreach (GameObject obj in aldoChildren)
        {
            obj.SetActive(false);
        }

        // Activate 2 random aldo children, if there are at least 2.
        if (aldoChildren.Count >= 3)
        {
            // Shuffle the list randomly
            Shuffle(aldoChildren);

            // Activate only the first two shuffled objects
            aldoChildren[0].SetActive(true);
            aldoChildren[1].SetActive(true);
            aldoChildren[2].SetActive(true);
        }
        else if (aldoChildren.Count == 1)
        {
            // If there is only one aldo child, activate it.
            aldoChildren[0].SetActive(true);
        }

        return aldoChildren;
    }
    // Helper function to shuffle a list randomly.
    private void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }






public void GameWin()

    {
        GameState = false;
        GameWinScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore(currentScore, 98);
    }

    public void GameOVer()
    {
       
        GameState = false;
        GameOverScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore(currentScore, 98);
    }

    public void GameResetScreen()
    {
        //  ActivateNextBackground();
        collisionCount = 0;
        progressBar.value = 10;
        tappedAldoCount = 0;
        ScoreText.text = "0";
        ResetFailedAttempts();
        Score.score = 0;
        currentScore = 0;
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        Player.Reset();
    }
    public void ResetFailedAttempts()
    {
        // Reset the failed attempts counter

        foreach (GameObject heart in hearts)
        {
            heart.SetActive(true);
        }
    }

    public void AddScore()
    {

        // Define the base score for a tap.
      //  float baseScore = 10f;

        // Calculate a multiplier from the current progress bar value.
        // Ensure progressBar and progressBar.maxValue are valid (set progressBar.maxValue via the Inspector).
        float timeMultiplier = (progressBar != null && progressBar.maxValue > 0) ? (progressBar.value / progressBar.maxValue) : 1f;

        // Compute the score increase. (Using Mathf.RoundToInt so the score is an integer.)
        int scoreIncrease = Mathf.RoundToInt(baseScore * timeMultiplier);

        // Update the score text.
        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += scoreIncrease;
            ScoreText.text = currentScore.ToString();
        }
        else
        {
            ScoreText.text = scoreIncrease.ToString();
        }
    }


    public void AddScore(float f)
    {
        Score.score += f;
    }



    //HELPER FUNTION TO GET SPAWN POINT
    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Transform local point to world space
        return SpawnBounds.transform.TransformPoint(randomPoint);
    }


    public struct ScoreObj
    {
        public float score;
    }
}
