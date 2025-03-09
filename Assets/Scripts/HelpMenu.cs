using UnityEngine;

public class HelpMenu : MonoBehaviour
{
    public GameObject helpMenu; // Drag your help menu here
    private float displayTime = 5f; // Display time

    private bool isShowing = false; // Used to track whether the menu is being displayed

    void Start()
    {
        // Show help menu at first and set timer
        ShowHelpMenu();
    }

    void Update()
    {
        // Detecting Tab key presses
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowHelpMenu();
        }
    }

    void ShowHelpMenu()
    {
        // Show Help Menu
        helpMenu.SetActive(true);
        isShowing = true;

        // Set a timer to hide after 5 seconds
        Invoke("HideHelpMenu", displayTime);
    }

    void HideHelpMenu()
    {
        // Hide Help Menu
        helpMenu.SetActive(false);
        isShowing = false;
    }
}
