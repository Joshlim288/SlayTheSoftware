using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour
{
    public GameObject mapPanel;
    public Sprite[] maps;

    // Start is called before the first frame update
    void Start()
    {
        Image mapImage = mapPanel.GetComponent<Image>();
        mapImage.sprite = maps[CurrentUser.getChosenMap()];
    }

    public void Exit(string exitToScene)
    {
        SceneManager.LoadScene(exitToScene);
    }
}
