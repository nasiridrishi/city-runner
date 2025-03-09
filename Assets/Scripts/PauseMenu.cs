using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    public static bool GameIsPaused = false;
    public GameObject PauseMenuUI;

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        GameIsPaused = true;
    }

    public void MainMenu()
    {
        Time.timeScale = 1.0f;
        GameIsPaused = false;
        SceneManager.LoadScene("StartMenuScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}