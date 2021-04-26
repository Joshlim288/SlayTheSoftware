using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that handles the logic of the ViewChallenges scene
/// </summary>
public class ViewChallenges : MonoBehaviour
{
    public Transform MainCanvas;
    public GameObject AllChallengesMenu;
    public GameObject NewChallengeMenu;
    public GameObject challengeTogglePrefab;
    public Transform challengePanel;
    public ToggleGroup toggleGroup;
    public Button editButton;
    public Button deleteButton;
    public Button shareButton;

    public InputField nameInput;

    private Toggle selectedToggle;

    /// <summary>
    /// Called when the scene is first loaded
    /// Initialize delete popup and sends a request to retrieve list of custom worlds from the server
    /// </summary>
    void Start()
    {
        AllChallengesMenu.SetActive(true);
        NewChallengeMenu.SetActive(false);
        
        StartCoroutine(DataManager.getCustomWorldList(displayChallengeData));

        // Create deletion popup for delete button
        Action action = () => {
            DeleteChallenge();
            SceneManager.LoadScene("ViewChallenges");
        };

        Action blankAction = () => { };

        deleteButton.onClick.AddListener(() =>
        {
            Popup popup = Popup.CreatePopup();

            popup.Init(MainCanvas,
                       "Do you really want to delete this Challenge?",
                       "No",
                       "Yes",
                       action,
                       blankAction
                      );
        });

        
    }

    /// <summary>
    /// Called once per frame
    /// Checks if any challenge is selected, and sets the interactable property of the button appropriately
    /// </summary>
    void Update()
    {
        // Disables the Edit button if no challenge is selected
        // Also updates the atrribute selectedToggle for retrieveSelectedWorld()
        if (toggleGroup.AnyTogglesOn())
        {
            editButton.interactable = true;
            deleteButton.interactable = true;
            shareButton.interactable = true;
            foreach (Toggle t in toggleGroup.ActiveToggles())
            {
                if (t.isOn == true)
                {
                    selectedToggle = t;
                }
            }
        }
    }

    /// <summary>
    /// Notify method called when Challenge Mode Worlds for the User have been retrieved
    /// Displays the name and access code of each World in a list
    /// </summary>
    public void displayChallengeData()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("Request Failed");
        }
        else
        {
            foreach (World w in EditData.customWorldList)
            {
                //Instantiate toggle using prefab and set toggle group
                GameObject challengeToggleObject = Instantiate(challengeTogglePrefab, challengePanel);
                challengeToggleObject.GetComponentInChildren<Toggle>().group = toggleGroup;

                //Retrieve text components and update based on world data
                TextMeshProUGUI[] texts = challengeToggleObject.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = w.world_name;
                texts[1].text = w.access_code;
            }
        }
    }

    /// <summary>
    /// Sends request to the server to creates a new Challenge
    /// </summary>
    public void CreateChallenge()
    {
        if (nameInput.text == "")
        {
            Debug.Log("nameInput field empty");
        }
        else
        {
            //Save details of new world, only sent to backend when questions are submitted
            EditData.customWorld = new World(nameInput.text, "Custom Challenge");
            EditData.isCreate = true;
            SceneManager.LoadScene("QuestionInterface");
        }
    }

    /// <summary>
    /// Called when the edit button is clicked
    /// Changes scene to QuestionInterface to edit questions
    /// </summary>
    public void EditChallenge()
    {
        retrieveSelectedWorld();
        //Indicate that user will be editing an existing challenge
        EditData.isCreate = false;
        SceneManager.LoadScene("QuestionInterface");
    }

    /// <summary>
    /// Called when the delete button is clicked
    /// Sends a request to the server to delete the World the Student has clicked on
    /// </summary>
    public void DeleteChallenge()
    {
        retrieveSelectedWorld();
        Debug.Log(EditData.customWorld.access_code);

        StartCoroutine(DataManager.deleteCustomWorld(EditData.customWorld.access_code, deleteNotify));
    }

    /// <summary>
    /// Called when edtiting or deleting a World
    /// Retrieves the selected world for editing/deleting based on chosen world in Challenge Panel.
    /// Updates CustomWorld attribute in CurrentUser's static class Edit Data.
    /// </summary>
    public void retrieveSelectedWorld()
    {
        // Retrieves actual game object attached to toggle to access data inside
        TextMeshProUGUI[] toggleTexts = selectedToggle.gameObject.GetComponentsInChildren<TextMeshProUGUI>();

        string selectedWorldAccessCode = toggleTexts[1].text;
         foreach (World w in EditData.customWorldList)
        {
            if (w.access_code == selectedWorldAccessCode)
            {
                EditData.customWorld = w;
            }
        }
    }

    /// <summary>
    /// Called when the Share button is clicked
    /// Allows the Student to share their Result on Facebook or Twitter
    /// </summary>
    public void ShareChallenge()
    {
        retrieveSelectedWorld();
        //Action to be passed into the Init function of the pop-up for Facebook sharing
        Action leftAction = () => {
            Application.OpenURL("https://www.facebook.com/sharer/sharer.php?kid_directed_site=0&sdk=joey&u=https://cz3003.kado.sg/share/challenge/?code=" + EditData.customWorld.access_code);
        };

        //Action to be passed into the Init function of the pop-up for Twitter sharing
        Action rightAction = () => {
            Application.OpenURL("https://twitter.com/intent/tweet?ref_src=twsrc%5Etfw&url=https://cz3003.kado.sg/share/challenge/?code=" + EditData.customWorld.access_code);
        };

        //Calls the static CreatePopup() function in the Popup script
        Popup popup = Popup.CreatePopup();

        //Initialise the popup
        popup.Init(MainCanvas,
                   "Which social media would you like to share to?",
                   "Facebook",
                   "Twitter",
                   rightAction,
                   leftAction
                  );
    }

    //////////////////////
    // Notify Callbacks //
    //////////////////////

    /// <summary>
    /// Notify method called when question request has been processed
    /// </summary>
    public void getQnNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("Question not retrieved");
        }
        else
        {
            SceneManager.LoadScene("QuestionInterface");
        }
    }

    /// <summary>
    /// Notify method called when deletion request has been processed
    /// </summary>
    public void deleteNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("World not deleted");
        }
    }
}