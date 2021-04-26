using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// For handling main menu logic and changing between menu scenes
/// </summary>
public class SceneChanger : MonoBehaviour
{
    /// <summary>
    /// Used for loading a new scene
    /// </summary>
    /// <param name="sceneName">Name of the scene to be loaded</param>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Used for initializing campaign mode
    /// sends a request to the server to retrieve all campaign worlds
    /// </summary>
    public void playCampaign()
    {
        StartCoroutine(DataManager.getCampaignWorlds(campaignNotify));
    }

    /// <summary>
    /// Notify method called when campaign world request is complete
    /// Changes the scene to WorldMap
    /// </summary>
    private void campaignNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            Debug.Log("Campaign Worlds Loaded"); 
            SceneManager.LoadScene("WorldMap");
        }
        else
        {
            Debug.Log("Request failed");
        }
    }

    /// <summary>
    /// Used for logging out of the Application
    /// Sends a logout request to the server
    /// </summary>
    public void logout()
    {
        StartCoroutine(DataManager.logout(logoutNotify));
    }

    /// <summary>
    /// Notify method to be called when logout request is complete
    /// Checks whether logout request was successful or not
    /// </summary>
    private void logoutNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            SceneManager.LoadScene("Login");
        }
        else
        {
            Debug.Log("Request failed");
        }
    }

    /// <summary>
    /// Used for closing the application
    /// First sends a logout request to the server before quitting
    /// </summary>
    public void quit()
    {
        StartCoroutine(DataManager.logout(Application.Quit));
    }
}
