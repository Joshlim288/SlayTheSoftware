using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the logic for the loading screen
/// Loads LevelMap asynchronously while displaying a loading bar
/// </summary>
public class LoadingScreen : MonoBehaviour 
{
    public static LoadingScreen instance;
    public GameObject LoadingPanel;
    public Slider ProgressBar;

    public float ProgressValue = 1.1f;
    public float ProgressMultipler_1 = 0.5f;
    public float ProgressMultipler_2 = 0.03f;

    public float load_level_time = 2f;

    void Awake()
    {
        MakeSingleton();
    }
    // Start is called before the first frame update
    void Start()
    {
        //ProgressBar = GetComponent<Slider>();
        //LoadingPanel = GameObject.Find("Canvas");
        StartCoroutine(SceneLoading());
    }

    void Update()
    {
        ShowLoadingScreen();
    }

    void MakeSingleton()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LoadLevel(string scene)
    {
        LoadingPanel.SetActive(true);
        ProgressValue = 0f;

        //Time.timeScale = 0f;
        SceneManager.LoadScene(scene);
    }

    void ShowLoadingScreen()
    {
        if (ProgressValue < 1f)
        {
            ProgressValue += ProgressMultipler_1 * ProgressMultipler_2;
            ProgressBar.value = ProgressValue;

            //loading bar has completed
            if (ProgressValue >= 1f)
            {
                ProgressValue = 1.1f;
                ProgressBar.value = 0f;
                LoadingPanel.SetActive(false);

                //Time.timeScale = 1f;
            }
        }
    }

    IEnumerator SceneLoading()
    {
        yield return new WaitForSeconds(load_level_time);


        //wait for unity to fill the assets in the scene
        LoadLevelAsync("LevelMap");
        
    }

    public void LoadLevelAsync(string scene)
    {
        StartCoroutine(LoadAsync(scene));
    }

    //IEnumerator - Delay the execution of a function by using yield return
    IEnumerator LoadAsync (string scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);

        LoadingPanel.SetActive(true);

        //operation not done
        while (!operation.isDone)
        {
            float progress = operation.progress / 0.9f;
            ProgressBar.value = progress;

            if (progress >= 1f)
            {
                LoadingPanel.SetActive(false);
            }

            yield return null; //skip one frame
        }
    }

    
}
