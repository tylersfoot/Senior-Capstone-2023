using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject canvas;
    public SettingsMenu settingsMenu; // for changing screens
    public InventoryMenu inventoryMenu;
    public bool isPaused = false;
    public string currentScreen = "none"; // which screen is the player seeing

    

    void Start()
    {
        canvas.SetActive(false);
    }

    public void Pause()
    {
        if (currentScreen != "gameOverMenu")
        {
            if (!isPaused)
            {
                // pauses the game
                Time.timeScale = 0f;
                canvas.SetActive(true);
                isPaused = true;
                currentScreen = "pauseMenu";
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                ResumeButton();
            }
        }
    }

    public void ResumeButton()
    {
        if (isPaused)
        {
            settingsMenu.Back(); // closes settings screen if open
            inventoryMenu.Back(); // closes inventory if open
            Time.timeScale = 1f; // resumes time
            canvas.SetActive(false); // disables pause menu canvas
            isPaused = false;
            currentScreen = "none";
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void InventoryButton()
    {
        if (currentScreen == "pauseMenu")
        {
            canvas.SetActive(false);
            inventoryMenu.OnOpen();
        }
    }

    public void SettingsButton()
    {
        if (currentScreen == "pauseMenu")
        {
            currentScreen = "settingsMenu";
            canvas.SetActive(false);
            settingsMenu.canvas.SetActive(true);
        }
    }

    public void MainMenuButton()
    {
        // load main menu
        GameDataManager.SaveData();
        Debug.Log("Saved Data!");
        SceneManager.LoadScene("MainMenu");
    }

    // public void SaveButton()
    // {
    //     GameDataManager.SaveData();
    //     Debug.Log("Saved Data!");
    // }

    // public void LoadButton()
    // {
    //     GameDataManager.LoadData();
    //     Debug.Log("Loaded Data!");
    // }
}

