using UnityEngine;

public class StartScreen : MonoBehaviour
{
    public GameObject startScreenUI; 
    public GameObject heroShip; 
    
    private bool gameStarted = false;

    void Start()
    {
        Time.timeScale = 0f; // Pause the game
        if (heroShip != null) heroShip.SetActive(false); 
    }

    void Update()
    {
        // Keep the Spacebar convenience!
        if (!gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    // Make this PUBLIC so the UI Button can see it
    public void StartGame()
    {
        if (gameStarted) return; // Prevent double clicking
        
        gameStarted = true;
        startScreenUI.SetActive(false); // Hide the UI
        Time.timeScale = 1f; // Unpause the game
        
        if (heroShip != null) heroShip.SetActive(true);
    }
}