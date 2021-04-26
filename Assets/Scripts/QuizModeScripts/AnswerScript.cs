using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script that handles behaviour of answer buttons
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AnswerScript : MonoBehaviour
{
    public int id;
    public QuizManager quizManager;
    public AudioSource playAudio;
    public AudioClip wrongAudio;
    public AudioClip clickAudio;
    //public GameObject ErrorBackground;
    public Color startColor;
    public GameObject Player;
    public GameObject BossGoblin;
    public Animator PlayerController;
    public Animator BossGoblinController;

    /// <summary>
    /// Called when QuizMode scene is loaded
    /// </summary>
    private void Start()
    {
        startColor = GetComponent<Image>().color;
        playAudio = GetComponent<AudioSource>();
        Player.SetActive(true);
        BossGoblin.SetActive(true);
        PlayerController.Play("PlayerIdle", -1, 0.0f);
        BossGoblinController.Play("GoblinIdle", -1, 0.0f);
    }

    /// <summary>
    /// Called when an answer button is clicked
    /// deactivates buttons and submits the answer to the server
    /// </summary>
    public void Answer()
    {
        quizManager.deactivateButtons();
        StartCoroutine(DataManager.answerQuestion(GameData.currentQuestion.record_id, id, answerNotify));
    }

    /// <summary>
    /// Notify method when status of answer has been retreived
    /// Plays the appropriate animation for the answer status
    /// </summary>
    public void answerNotify()
    {
        if (DataManager.getRequestStatus() == true)
        {
            if(GameData.currentQuestion.answered_correctly == true)
            {
                GetComponent<Image>().color = Color.green;
                playAudio.clip = clickAudio;
                playAudio.Play();
                quizManager.correct();
                PlayerController.Play("PlayerAttack", -1, 0.0f);
                BossGoblinController.Play("GoblinDie", -1, 0.0f);
            }
            else
            {
                GetComponent<Image>().color = Color.red;
                playAudio.clip = wrongAudio;
                playAudio.Play();
                quizManager.wrong();
                PlayerController.Play("PlayerDie", -1, 0.0f);
                BossGoblinController.Play("GoblinAttack", -1, 0.0f);
            }
        }
        else 
        {
            Debug.Log("Request Failed");
        }
    }
}
