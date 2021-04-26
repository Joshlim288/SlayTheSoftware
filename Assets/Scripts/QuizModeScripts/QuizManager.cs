using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// Script for handling the logic in normal quiz mode
/// </summary>
public class QuizManager : MonoBehaviour
{
    public Button[] options;
    public float ButtonReactivateDelay = 1f;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject GoPanel;

    public Text QuestionTxt;
    public Text ScoreTxt;
    public Text DifficultyTxt;

    public Text PointsChangeTxt;
    public Text CorrectTxt;

    public int pointsChange = 0;
    public int correctCount = 0;

    /// <summary>
    /// Called when the scene is loaded
    /// Sends a request to the backend for question data
    /// </summary>
    private void Start()
    {
        GoPanel.SetActive(false);
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        if (GameData.bgMap <= 2)
        {
            DifficultyTxt.text = CurrentUser.currentDifficulty;
        }
        else
        {
            DifficultyTxt.text = "-";
        }
        StartCoroutine(DataManager.getQuestionData(CurrentUser.currentWorldId, generateQuestionNotify));
    }

    /// <summary>
    /// Notify method called when questions for the Level have been retrieved
    /// Sets the question text in the scene
    /// </summary>
    public void generateQuestionNotify()
    {
        GameData.currentQuestion = GameData.currentQuestions[0];
        QuestionTxt.text = GameData.currentQuestion.question;
        pointsChange = GameData.levelScore;
        correctCount = GameData.correctCount;
        SetAnswers();
        foreach (Button answer in options)
        {
            answer.interactable = true;
        }
    }
    /// <summary>
    /// Sets the answer texts in the scene
    /// </summary>
    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;
            options[i].transform.GetChild(0).GetComponent<Text>().text = GameData.currentQuestion.answers[i].answer;
            options[i].GetComponent<AnswerScript>().id = GameData.currentQuestion.answers[i].id;
        }
    }

    /// <summary>
    /// Called when Level has been completed
    /// Displays summary panel for the level
    /// </summary>
    void GameOver()
    {
        PointsChangeTxt.text = pointsChange.ToString();
        CorrectTxt.text = correctCount.ToString();
        
        Quizpanel.SetActive(false);
        GoPanel.SetActive(true);
    }

    /// <summary>
    /// Called when question has been answered correctly
    /// adjusts points and correct counter accordingly
    /// </summary>
    public void correct()
    {
        //when you answer correct
        pointsChange += GameData.currentQuestion.points_change;
        correctCount++;
        CurrentUser.currentScore += GameData.currentQuestion.points_change;
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        StartCoroutine(waitForNext());
    }

    /// <summary>
    /// Called when question has been answered incorrectly
    /// adjusts points and correct counter accordingly
    /// </summary>
    public void wrong()
    {
        //when you answer wrong
        pointsChange += GameData.currentQuestion.points_change;
        CurrentUser.currentScore += GameData.currentQuestion.points_change;
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        StartCoroutine(waitForNext());
    }

    /// <summary>
    /// Called when an answer has been clicked
    /// Ensures only one answer will be logged for every question
    /// </summary>
    public void deactivateButtons()
    {
        // When the user clicks the answer, all buttons will be non-interactable
        foreach (Button answer in options)
        {
            answer.interactable = false;
        }
    }

    IEnumerator waitForNext()
    {
        yield return new WaitForSeconds(1);
        generateQuestion();
        
    }

    /// <summary>
    /// Called when a question has been answered and the animation has been displayed
    /// Creates a request to retrieve the next question from the server
    /// </summary>
    void generateQuestion()
    {
        if(GameData.currentQuestion.index <= 1)
        {
            StartCoroutine(DataManager.getQuestionData(CurrentUser.currentWorldId, generateQuestionNotify));
        }
        else
        {
            Debug.Log("Level Complete");
            GameOver();
        }
    }

    /// <summary>
    /// Called when back button is pressed
    /// Returns to the LevelMap
    /// </summary>
    public void exit()
    {
        Debug.Log(CurrentUser.currentWorldId);
        if (CurrentUser.currentWorldId < 3)
        {
            StartCoroutine(DataManager.getCurrentPosition(userPositionNotify));
        }
        else
        {  
            StartCoroutine(DataManager.getCurrentPosition(userPositionNotify, CurrentUser.currentWorldId));
        }
    }

    /// <summary>
    /// Notify method called when user position has been retrieved
    /// Loads the LevelMap
    /// </summary>
    public void userPositionNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            SceneManager.LoadScene("LoadingScreen");
        }
        else 
        {
            Debug.Log("Request Failed");
        }
        
    }
}
