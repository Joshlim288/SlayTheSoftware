using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the answer button behaviour for the boss quiz mode
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BossAnsScript : MonoBehaviour
{
    public int id;
    public bool isCorrect = false;
    public BossQuizManager quizManager;
    public AudioSource playAudio;
    public AudioClip clickAudio;
    public Color startColor;
    public GameObject Player;
    public ParticleSystem CodexGlow;

    /// <summary>
    /// Called when the scene is first loaded
    /// Sets up the animations for answering a Question
    /// </summary>
    private void Start()
    {
        startColor = GetComponent<Image>().color;
        playAudio = GetComponent<AudioSource>();
        Player.SetActive(true);
        CodexGlow.Stop();
    }

    /// <summary>
    /// Called when an answer button is clicked
    /// Plays the answering animation and calls the answer method in BossQuizManager
    /// </summary>
    public void Answer()
    {
        GetComponent<Image>().color = Color.cyan;
        Debug.Log("Question Answered");
        playAudio.clip = clickAudio;
        playAudio.Play();
        Debug.Log(GameData.currentQuestion.record_id.ToString() + id.ToString());
        Debug.Log(quizManager.currentQuestion);
        quizManager.recordIds[quizManager.currentQuestion] = GameData.currentQuestion.record_id;
        quizManager.answerIds[quizManager.currentQuestion] = id;
        quizManager.Answer();
        CodexGlow.Play();
    }
}
