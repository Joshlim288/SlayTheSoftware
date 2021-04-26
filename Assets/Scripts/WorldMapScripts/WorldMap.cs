using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script to handle World Map logic
/// </summary>
public class WorldMap : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Button button3;

    /// <summary>
    /// Called when the scene is loaded
    /// Sends a request to the server to retrieve Student's current position
    /// </summary>
    void Start()
    {
        // We first lock the 2nd and 3rd worlds in case they have not been unlocked
        button2.interactable = false;
        button3.interactable = false;
        StartCoroutine(DataManager.getCurrentPosition(lockWorldNotify));
    }

    /// <summary>
    /// Notify method called when the position retreival request has been completed
    /// Checks on the Student's current position and locks/unlocks the World button accordingly
    /// </summary>
    private void lockWorldNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            switch(CurrentUser.currentWorldId)
            {
                case 1:
                    button2.interactable = false;
                    button3.interactable = false;
                    break;
                case 2:
                    button2.interactable = true;
                    button3.interactable = false;
                    break;
                default:
                    button2.interactable = true;
                    button3.interactable = true;
                    break;
            }
        }
        else
        {
            Debug.Log("Request failed");
        }
        
    }

    /// <summary>
    /// Called when Student selects a World
    /// First sets Student's current position appropriately, then changes the scene to LoadingScreen to load the Level Map
    /// </summary>
    /// <param name="MapNum"></param>
    public void ChangeMap(int MapNum)
    {
        if (MapNum+1 < CurrentUser.currentWorldId)
        {
            // Student has selected a completed world, set current position to the last level
            CurrentUser.currentWorldId = GameData.campaignWorlds[MapNum].id;
            CurrentUser.currentSectionId = 3*CurrentUser.currentWorldId;
            CurrentUser.currentLevelId = 9;
            CurrentUser.hasCompleted = true;
        }
        // set background map of LevelMap to chosen map
        GameData.bgMap = MapNum;
        GameData.currentWorld = GameData.campaignWorlds[MapNum];
        CurrentUser.sceneToLoad = "LevelMap";
        SceneManager.LoadScene("LoadingScreen");
    }

    /// <summary>
    /// Called when selecting the back button
    /// Returns to main menu
    /// </summary>
    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
