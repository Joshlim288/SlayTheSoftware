using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Script to handle the management of Questions in CreateChallenge scene
/// </summary>
public class QuestionInterface : MonoBehaviour
{
    public Transform mainCanvas;
    public Button backButton;
    public TextMeshProUGUI questionCountText;
    public InputField questionStatement;
    public InputField[] answerList;
    public Toggle[] toggleList;

    const int NumberOfQns = 12; //12 questions per Challenge

    int counter; //Used for iterate through submitting of questions to backend
    int sectionID; //Used for sending questions for a newly created world
    int questionIndex; //Used for keeping track of current question active

    /// <summary>
    /// Called when the scene is first loaded
    /// Sets cusomQuestions in EditData to the appropriate value based on whether the User has selected create or edit
    /// </summary>
    void Start()
    {
        questionIndex = 0;
        counter = 0;

        if (EditData.isCreate) EditData.customQuestions = new Question[NumberOfQns]; //Initialise an array to accomodate the quiz questions
        else StartCoroutine(DataManager.getCustomQuestions(EditData.customWorld.id, getQuestionsNotify)); //Retreive current questions in existing world from backend

        //Action to be passed into the Init function of the pop-up
        Action action = () => {
            SceneManager.LoadScene("ViewChallenges");
        };

        Action blankAction = () => { };

        //add listener
        backButton.onClick.AddListener(() =>
        {
            //Calls the static CreatePopup() function in the Popup script
            Popup popup = Popup.CreatePopup();

            //Initialise the popup
            popup.Init(mainCanvas,
                       "Data entered will be lost, continue?",
                       "No",
                       "Yes",
                       action,
                       blankAction
                      );
        });

    }

    /// <summary>
    /// Notify method called when Challenge Mode World questions have been retrieved
    /// </summary>
    public void getQuestionsNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("Request Failed");
        }
        else
        {
            Debug.Log("existing questions retrieved");
            fillInputFields();
        }
    }

    /// <summary>
    /// Used to populate input fields with question data.
    /// </summary>
    public void fillInputFields()
    {
        int i = 0;

        Debug.Log(questionIndex);
        Debug.Log(EditData.customQuestions[questionIndex]);

        //If Question instance does not exist yet, clear input fields
        //Used when creating new world AND when question has not been entered yet
        if (EditData.customQuestions[questionIndex] == null)
        {
            questionStatement.text = "";
            for (i = 0; i < 4; i++)
            {
                answerList[i].text = "";
                toggleList[i].isOn = false;
            }
        }
        //If Question instance exists already, update input fields with Question's data
        //Used when editing existing world OR when creating new world but question has been entered already
        else
        {
            questionStatement.text = EditData.customQuestions[questionIndex].question;

            foreach (Answer option in EditData.customQuestions[questionIndex].answers)
            {
                answerList[i].text = option.answer;
                toggleList[i].isOn = option.is_correct;
                i++;
            }
        }
    }

    /// <summary>
    /// To update the questions after moving to another question in the interface
    /// </summary>
    public void saveToQuestionList()
    {
        Debug.Log(EditData.customQuestions);
        int i = 0;

        //If Question instance does not exist yet, create new Question and Answer instances
        //Used when creating new world AND when question has not been entered yet
        if (EditData.customQuestions[questionIndex] == null)
        {
            string questionText = questionStatement.text;
            Answer[] ansArray = new Answer[4];

            EditData.customQuestions[questionIndex] = new Question(questionStatement.text, ansArray);

            for (i = 0; i < 4; i++) ansArray[i] = new Answer(answerList[i].text, toggleList[i].isOn);
        }
        //For existing world, just update the Question and Answer instances' directly
        else
        {
            EditData.customQuestions[questionIndex].question = questionStatement.text;

            foreach (Answer option in EditData.customQuestions[questionIndex].answers)
            {
                option.answer = answerList[i].text;
                option.is_correct = toggleList[i].isOn;
                i++;
            }
        }
    }

    /// <summary>
    /// Called when user moves to the next question
    /// Saves current question details and fetch the next question details
    /// Increments and updates question counter at bottom of scene
    /// </summary>
    public void nextQuestion()
    {
        if(questionIndex < NumberOfQns - 1)
        {
            saveToQuestionList();
            questionIndex++;
            fillInputFields();
            questionCountText.text = "QN " + (questionIndex + 1).ToString() + "/" + NumberOfQns.ToString();  
        }
    }

    /// <summary>
    /// Called when user moves to the previous question
    /// Saves current question details and fetch the previous question details
    /// Decrements and updates question counter at bottom of scene
    /// </summary>
    public void previousQuestion()
    {
        if (questionIndex > 0)
        {
            saveToQuestionList();
            questionIndex--;
            fillInputFields();
            questionCountText.text = "QN " + (questionIndex + 1).ToString() + "/" + NumberOfQns.ToString();
        }
    }

    /// <summary>
    /// Used to submit or update edited questions to the server
    /// </summary>
    public void recursiveSubmit()
    {
        if (counter < NumberOfQns)
        {
            Debug.Log(counter + "st iteration");
            if (EditData.isCreate) StartCoroutine(DataManager.sendQuestion(EditData.customQuestions[counter], sectionID, recursiveNotify));
            else StartCoroutine(DataManager.updateQuestion(EditData.customQuestions[counter], EditData.customQuestions[counter].id, recursiveNotify));
        }
        else
        {
            Debug.Log("completed.");

            //Create notification informing user that world has been created/edited successfully
            Action action = () => {
                SceneManager.LoadScene("ViewChallenges");
            };

            Notif successNotif = Notif.CreateNotif();
            string message;
            if (EditData.isCreate) message = "Your world has been created successfully!";
            else message = "Your world has been edited successfully!";

            //Initialise the notif
            successNotif.Init(mainCanvas,
                       message,
                       "Okay",
                       action
                      );
        }
    }

    /// <summary>
    /// Notify method called when create or update question request has been processed
    /// </summary>
    public void recursiveNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("Request Failed");
        }
        else
        {
            //success notif
            counter++;
            Debug.Log("question submitted");
            Debug.Log(counter);
            recursiveSubmit();
        }
    }

    /// <summary>
    /// Notify method called when the request to create a World has been processed
    /// </summary>
    public void createWorldNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("world not created!");
        }
        else
        {
            Debug.Log("world created!");
            sectionID = EditData.customWorld.sections[0].id;
            recursiveSubmit();
        }
    }

    /// <summary>
    /// Submits questions to server
    /// </summary>
    public void submitQuestions()
    {
        saveToQuestionList(); //The question currently active on the scene needs to be saved as well
        Debug.Log(sectionID);


        if (checkIfValid())
        {
            if (EditData.isCreate) StartCoroutine(DataManager.createWorld(EditData.customWorld.world_name, EditData.customWorld.topic, createWorldNotify));
            else recursiveSubmit();
        }
        else
        {
            //Create notification informing user that world is not valid
            Action action = () => {};

            Notif failureNotif = Notif.CreateNotif();

            //Initialise the notif
            failureNotif.Init(mainCanvas,
                       "Not all fields have been filled in, please check again.",
                       "Okay",
                       action
                      );

        }
    }

    /// <summary>
    /// Basic check to ensure that all fields have been filled in before sending to backend
    /// Each question should only have one correct option as well
    /// </summary>
    /// <returns> boolean variable indicating if all questions are valid</returns>
    public bool checkIfValid()
    {
        int correctCount;
        foreach (Question q in EditData.customQuestions)
        {
            if (q == null) return false;
            correctCount = 0;
            if (q.question == null || q.question == "") return false;
            foreach (Answer a in q.answers)
            {
                if (a.answer == null || a.answer == "") return false;
                if (a.is_correct == true) correctCount++;
            }
            if (correctCount != 1) return false;
        }
        return true;
    }
}

