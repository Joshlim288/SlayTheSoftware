using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMap : MonoBehaviour
{
    public void ChangeMap(int MapNum)
    {
        CurrentUser.setChosenMap(MapNum);
        SceneManager.LoadScene("LevelMap");
    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
