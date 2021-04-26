using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for verifying assignment or challenge mode codes
/// </summary>
public class CodeVerifier : MonoBehaviour
{
    public InputField codeInput;
    public GameObject invalidPrompt;

    /// <summary>
    /// Sends a request to the server to verify a World access code
    /// </summary>
    /// <param name="type">can be "assignment" or "challenge" - Only worlds access codes matching this type will be checked</param>
    public void Verify(string type)
    {
        if (codeInput.text == "")
        {
            invalidPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            invalidPrompt.SetActive(true);
        }
        else StartCoroutine(DataManager.getCustomWorld(codeInput.text, customWorldNotify, type));
    } 

    /// <summary>
    /// Notify method called when an access code request has been processed
    /// if successful, sends a request to the server to retrieve Student's current position in that World
    /// </summary>
    public void customWorldNotify()
    {
        
        if (DataManager.getRequestStatus() == true)
        {
            GameData.currentWorld = EditData.customWorld;
            StartCoroutine(DataManager.getCurrentPosition(positionNotify, GameData.currentWorld.id));
        }
        else
        {
            invalidPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            invalidPrompt.SetActive(true);
        }
    }

    /// <summary>
    /// Notiify method called when the Student's poition has been retrieved
    /// Loads the LeveMap scene
    /// </summary>
    public void positionNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            GameData.bgMap = 3;
            CurrentUser.sceneToLoad = "LevelMap";
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Debug.Log("Request Failed");
        }
        
    }
}
