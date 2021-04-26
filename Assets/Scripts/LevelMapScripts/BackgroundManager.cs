using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that handles the changing of backgrounds in LevelMap
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    public GameObject mapPanel;
    public Sprite[] maps;

    /// <summary>
    /// Start is called when the scene is first loaded
    /// Retrieves and loads the background sprite for the chosen map
    /// </summary>
    void Start()
    {
        Image mapImage = mapPanel.GetComponent<Image>();
        mapImage.sprite = maps[GameData.bgMap];
    }

    /// <summary>
    /// Called when back button is clicked in the LevelMap
    /// Loads the scene based on the type of World it is
    /// </summary>
    public void Exit()
    {
        if (GameData.bgMap == 3)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("WorldMap");
        }
    }
}
