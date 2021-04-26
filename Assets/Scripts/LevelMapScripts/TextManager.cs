using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Script to handle the information text in LevelMap
/// </summary>
public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI PositionObject;
    public TextMeshProUGUI TopicObject;
    public TextMeshProUGUI CurrentScoreObject;
    public TextMeshProUGUI TotalScoreObject;
    public TextMeshProUGUI DifficultyObject;
    private string PositionText;

    /// <summary>
    /// Called when LevelMap scene is loaded
    /// Sends a request to the server for the User's score in the World
    /// </summary>
    public void Start()
    {
        // Set topic text
        if (CurrentUser.currentWorldId <= 3)
        {
            PositionObject.text = "World " + CurrentUser.currentWorldId + " - " + "Section " + CurrentUser.currentSectionId;
        }
        else
        {
            PositionObject.text = GameData.currentWorld.world_name;
        }
        foreach(Section sect in GameData.currentWorld.sections)
        {
            if (sect.id == CurrentUser.currentSectionId)
            {
                TopicObject.text = sect.sub_topic_name;
            }
        }
        
        // Set current score
        Debug.Log("current world ID: " + CurrentUser.currentWorldId.ToString());
        StartCoroutine(DataManager.getWorldScore(CurrentUser.currentWorldId, currentScoreNotify));
    }

    /// <summary>
    /// Notify method for when the World score has been retrieved
    /// If the World is a Challenge Mode World, the difficulty and total score will be set and undefined
    /// Else, a request is sent to the server for the total score that the User has attained
    /// </summary>
    private void currentScoreNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            CurrentScoreObject.text = CurrentUser.currentScore.ToString();
            // Set total score
            if (GameData.bgMap < 3)
            {
                StartCoroutine(DataManager.getUserResult(CurrentUser.loggedIn.id, totalScoreNotify));
            }
            else
            {
                TotalScoreObject.text = "-";
                DifficultyObject.text = "-";
            }
            
        }
        else
        {
            Debug.Log("Request failed");
        }
    }

    /// <summary>
    /// Notify method called when the total score has been retrieved
    /// Sends a request to the server to retrieve the difficulty of the questions in the World for the User
    /// </summary>
    private void totalScoreNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            TotalScoreObject.text = LeaderboardData.userResult.points.ToString();
            // Set Difficulty text
            StartCoroutine(DataManager.getWorldDifficulty(CurrentUser.currentWorldId, difficultyNotify));
        }
        else
        {
            Debug.Log("Request failed");
        }
        
    }

    /// <summary>
    /// Notify method called when the difficulty has been retrieved
    /// </summary>
    private void difficultyNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            DifficultyObject.text = CurrentUser.currentDifficulty;
        }
        else
        {
            Debug.Log("Request failed");
        }
        
    }
}
