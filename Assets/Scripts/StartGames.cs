using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGames : MonoBehaviour
{
    public void StartMenu()
    {
        SceneManager.LoadScene("Main Scene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}