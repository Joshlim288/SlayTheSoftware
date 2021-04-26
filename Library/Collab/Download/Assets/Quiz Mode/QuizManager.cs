using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public List<QuestionAndAnswers> QnA;
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

    private void Start()
    {
        GoPanel.SetActive(false);
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        if (CurrentUser.currentWorldId < 3)
        {
            DifficultyTxt.text = CurrentUser.currentDifficulty;
        }
        else
        {
            DifficultyTxt.text = "-";
        }
        

        StartCoroutine(DataManager.getQuestionData(CurrentUser.currentWorldId, generateQuestionNotify));
    }

    public void retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GameOver()
    {
        PointsChangeTxt.text = pointsChange.ToString();
        CorrectTxt.text = correctCount.ToString();
        
        Quizpanel.SetActive(false);
        GoPanel.SetActive(true);
    }

    public void correct()
    {
        //when you answer correct
        // When the user clicks the answer, all buttons will be non-interactable
        foreach (Button answer in options)
        {
            answer.interactable = false;
        }
        //Delay the buttons before it is interactable again
        StartCoroutine(EnableButtonAfterDelay(options, ButtonReactivateDelay)); 
        
        //CurrentUser.answeredQuestion();
        //ScoreTxt.text = CurrentUser.userResult.points.ToString();
        // QnA.RemoveAt(currentQuestion);
        pointsChange += GameData.currentQuestion.points_change;
        correctCount++;
        CurrentUser.currentScore += GameData.currentQuestion.points_change;
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        StartCoroutine(waitForNext());
    }

    public void wrong()
    {
        //when you answer wrong
        foreach (Button answer in options)
        {
            answer.interactable = false;
        }
        //Delay the buttons before it is interactable again
        StartCoroutine(EnableButtonAfterDelay(options, ButtonReactivateDelay)); 
        // QnA.RemoveAt(currentQuestion);
        pointsChange += GameData.currentQuestion.points_change;
        CurrentUser.currentScore += GameData.currentQuestion.points_change;
        ScoreTxt.text = CurrentUser.currentScore.ToString();
        StartCoroutine(waitForNext());
    }

    IEnumerator EnableButtonAfterDelay(Button[] options, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (Button answer in options)
        {
            answer.interactable = true;
        }

    }

    IEnumerator waitForNext()
    {
        yield return new WaitForSeconds(1);
        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;
            // options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>().text = GameData.currentQuestion.answers[i].answer;
            options[i].GetComponent<AnswerScript>().id = GameData.currentQuestion.answers[i].id;
            
            // if(QnA[currentQuestion].CorrectAnswer == i+1)
            // {
            //     options[i].GetComponent<AnswerScript>().isCorrect = true;
            // }
        }
    }

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

    public void generateQuestionNotify()
    {
        GameData.currentQuestion = GameData.currentQuestions[0];
        QuestionTxt.text = GameData.currentQuestion.question;
        pointsChange = GameData.levelScore;
        correctCount = GameData.correctCount;
        SetAnswers();
    }

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
