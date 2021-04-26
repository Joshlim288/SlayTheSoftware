using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that handles the logic for answering questions in BossQuizMode
/// </summary>
public class BossQuizManager : MonoBehaviour
{
    public Button[] options;
    public float ButtonReactivateDelay = 1f;
    public int currentQuestion;
    public int[] recordIds;
    public int[] answerIds;

    public GameObject Quizpanel;
    public GameObject PassPanel;
    public GameObject FailPanel;
    public SpriteRenderer Codex;
    public Sprite CodexFill;
    public SpriteRenderer FailInterface;
    public Sprite Kaboom;
    public AudioSource KaboomAudio;

    public Text QuestionTxt;
    public Text CorrectQuestionsTxt;
    //public Text ScoreTxt;

    public GameObject CodexCanvas;
    public Slider CodexBar;
    //public Color pointColor;
    public ParticleSystem CodexKaboom;
    public ParticleSystem CodexG_Kaboom;

    public float ProgressValue = 0f;
    public float ProgressMultipler = 0.1f;

    int totalQuestions = 0;
    public int QCorrect;
    public int score;

    /// <summary>
    /// Called when the scene is first loaded
    /// Sets up the animations and audio for the scene, and retrieves questions from the server
    /// </summary>
    private void Start()
    {
        PassPanel.SetActive(false);
        FailPanel.SetActive(false);
        KaboomAudio = GetComponent<AudioSource>();
        CorrectQuestionsTxt.GetComponent<Text>().enabled = false;
        CorrectQuestionsTxt.GetComponent<Text>().color = Color.white;
        CodexKaboom.Stop();
        CodexG_Kaboom.Stop();
        StartCoroutine(DataManager.getQuestionData(CurrentUser.currentWorldId, loadBossQuestions));
    }

    /// <summary>
    /// Notify method called when the questions have been successfully retrieved
    /// Sets the length of the boss level and generates the first question
    /// </summary>
    public void loadBossQuestions()
    {
        if (DataManager.getRequestStatus() == true)
        {
            totalQuestions = GameData.currentQuestions.Length;
            recordIds = new int[GameData.currentQuestions.Length];
            answerIds = new int[GameData.currentQuestions.Length];
            generateQuestion();
        }
        else
        {
            Debug.Log("Request Failed");
        }
        
    }

    /// <summary>
    /// Called when the Student has failed the boss level and presses the "retry" button on the failure popup
    /// Reloads the scene and restarts the boss level with new questions
    /// </summary>
    public void retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Called when all questions in the boss level have been answered
    /// Plays the appropriate animations and enables the appropriate popup, depending on if they have passed or failed
    /// </summary>
    void GameOver()
    {
        Quizpanel.SetActive(false);
        CorrectQuestionsTxt.GetComponent<Text>().enabled = true;
        CorrectQuestionsTxt.text = GameData.bossCorrectQuestions.ToString();
        if (GameData.bossCorrectQuestions >= totalQuestions/2)
        {
           PassPanel.SetActive(true);
           CodexG_Kaboom.Play();
           CorrectQuestionsTxt.GetComponent<Text>().color = Color.green;
           Codex.GetComponent<SpriteRenderer>().sprite = CodexFill;
        }
        else
        {
           CorrectQuestionsTxt.GetComponent<Text>().color = Color.red;
           FailPanel.SetActive(true);
           FailInterface.GetComponent<SpriteRenderer>().sprite = Kaboom;
           CodexKaboom.Play();
           KaboomAudio.Play();
        }
    }

    /// <summary>
    /// Called from BossAnsScript when the Student has selected an answer
    /// </summary>
    public void Answer()
    {
        // When the user clicks the answer, all buttons will be non-interactable
        foreach (Button answer in options)
        {
            answer.interactable = false;
        }
        //Delay the buttons before it is interactable again
        StartCoroutine(EnableButtonAfterDelay(ButtonReactivateDelay));;

        //when you are right
        if (ProgressValue >= 0f && ProgressValue < 1f)
        {
            ProgressValue += ProgressMultipler;
            CodexBar.value = ProgressValue;
        }
        StartCoroutine(waitForNext());
    }

    /// <summary>
    /// Re-enables buttons for clicking after a delay, to leave time for the questions to be loaded
    /// </summary>
    /// <param name="seconds"></param>
    IEnumerator EnableButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (Button answer in options)
        {
            answer.interactable = true;
        }

    }

    /// <summary>
    /// Used to create a 1 second delay
    /// </summary>
    IEnumerator waitForNext()
    {
        yield return new WaitForSeconds(1);
        generateQuestion();
    }

    /// <summary>
    /// Sets the answer text and colour of all answer buttons, based on the current question
    /// </summary>
    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Image>().color = options[i].GetComponent<BossAnsScript>().startColor;
            options[i].transform.GetChild(0).GetComponent<Text>().text = GameData.currentQuestion.answers[i].answer;
            options[i].GetComponent<BossAnsScript>().id = GameData.currentQuestion.answers[i].id;

        }
    }

    /// <summary>
    /// Moves to the next question
    /// Set the question text and changes currentQuestion in GameData
    /// </summary>
    void generateQuestion()
    {
        currentQuestion++;
        if (currentQuestion < GameData.currentQuestions.Length)
        {
            QuestionTxt.text = GameData.currentQuestions[currentQuestion].question;
            GameData.currentQuestion = GameData.currentQuestions[currentQuestion];
            SetAnswers();
        }
        else
        {
            Debug.Log("Out of Questions");
            StartCoroutine(DataManager.answerBossQuestions(recordIds, answerIds, GameOver));
        }
    }

    /// <summary>
    /// Called when the back button is pressed
    /// Loads and changes to the LevelMap
    /// </summary>
    public void exitToLevelMap()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    /// <summary>
    /// Called when the Student has completed the Boss Level
    /// Retrieves the new position of the Student from the server
    /// </summary>
    public void exit()
    {
        Debug.Log("Boss Level Complete");
        StartCoroutine(DataManager.getCurrentPosition(userPositionNotify));
    }

    /// <summary>
    /// Notify method called when the new position of the Student has been retrieved
    /// Loads and changes scene to the WorldMap
    /// </summary>
    public void userPositionNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            SceneManager.LoadScene("WorldMap");
        }
        else 
        {
            Debug.Log("Request Failed");
        }
        
    }
}
