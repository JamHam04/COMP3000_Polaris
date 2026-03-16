using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Canvas
    public Canvas mainMenuCanvas;

    // Panels
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    // Buttons
    public Button playButton;
    public Button levelSelectButton;
    public Button backButton;

    // 8 Level Select Buttons
    public Button[] levelButtons;


    void Start()
    {
       if (mainMenuCanvas == null)
       {
           mainMenuCanvas = FindObjectOfType<Canvas>();
       }

        // Panel Setup
        mainMenuPanel = mainMenuCanvas.transform.Find("MainPanel").gameObject;
        levelSelectPanel = mainMenuCanvas.transform.Find("LevelPanel").gameObject;
        // Button Setup
        playButton = mainMenuPanel.transform.Find("PlayButton").GetComponent<Button>();
        levelSelectButton = mainMenuPanel.transform.Find("LevelSelectButton").GetComponent<Button>();
        backButton = levelSelectPanel.transform.Find("BackButton").GetComponent<Button>();

       
        // Button Listeners
        playButton.onClick.AddListener(StartGame);
        levelSelectButton.onClick.AddListener(OpenLevelSelect);
        backButton.onClick.AddListener(OpenMainMenu);

 


        int levelIndex = 1;
        foreach (Button btn in levelButtons)
        {
            int index = levelIndex; 
            btn.onClick.AddListener(() => LoadLevel(index));
            levelIndex++;
        }
    



    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OpenMainMenu()
    {
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }

}
