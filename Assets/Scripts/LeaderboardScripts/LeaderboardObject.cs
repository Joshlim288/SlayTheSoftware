using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that handles the behaviour of the Leaderboard
/// </summary>
public class LeaderboardObject : MonoBehaviour
{
    private int chosenWorldID = 0; //by default, "ALL WORLDS" option is selected
    List<World> worldOptions = new List<World>();

    public Transform MainCanvas;
    public GameObject leaderboardRowPrefab;
    public Transform leaderboardPanel;
    public Button shareButton;

    public TMP_Dropdown dropdownMenu;

    /// <summary>
    /// Called when the scene is loaded
    /// Initializes popups, sends a request to the
    /// </summary>
    void Start()
    {
        // Creates pop-up confirming if user wants to share to Twitter or Facebook
        // Action to be passed into the Init function of the pop-up for Facebook sharing
        Action leftAction = () => {
            if (chosenWorldID == 0)
            {
                Application.OpenURL("https://www.facebook.com/sharer/sharer.php?kid_directed_site=0&sdk=joey&u=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fpid%3D"
                + CurrentUser.loggedIn.id.ToString());
            }
            else
            {
                Application.OpenURL("https://www.facebook.com/sharer/sharer.php?kid_directed_site=0&sdk=joey&u=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fwid%3D"
                                    + chosenWorldID.ToString() + " %26pid%3D" + CurrentUser.loggedIn.id.ToString());
            }
        };

        //Action to be passed into the Init function of the pop-up for Twitter sharing
        Action rightAction = () => {
            if (chosenWorldID == 0)
            {

                Application.OpenURL("https://twitter.com/intent/tweet?ref_src=twsrc%5Etfw&url=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fpid%3D"
                + CurrentUser.loggedIn.id.ToString());
            }
            else
            {
                Application.OpenURL("https://twitter.com/intent/tweet?ref_src=twsrc%5Etfw&url=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fwid%3D" 
                                    + chosenWorldID.ToString() + " %26pid%3D" + CurrentUser.loggedIn.id.ToString());
            }
        };

        shareButton.onClick.AddListener(() =>
        {
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

        });

        StartCoroutine(DataManager.getCampaignWorlds(getCampaignNotify));
    }

    /// <summary>
    /// Called when the request for retrieving campaign worlds has been processed
    /// Sends a request to retrieve the list of custom worlds
    /// </summary>
    private void getCampaignNotify()
    {
        if (DataManager.getRequestStatus() == true) StartCoroutine(DataManager.getCustomWorldList(loadDropdown));
        else Debug.Log("Request failed");
    }
    
    /// <summary>
    /// Populate dropdown options with "All Worlds" option, campaign worlds and custom worlds
    /// </summary>
    private void loadDropdown()
    {
        if (DataManager.getRequestStatus() == true)
        {
            dropdownMenu.options.Clear();
            worldOptions.AddRange(GameData.campaignWorlds);
            worldOptions.AddRange(EditData.customWorldList);

            dropdownMenu.options.Add(new TMP_Dropdown.OptionData() { text = "ALL WORLDS" }); //Add "ALL WORLDS" as first option
            foreach (var world in worldOptions) dropdownMenu.options.Add(new TMP_Dropdown.OptionData() { text = world.world_name });

            dropdownMenu.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdownMenu); });

            getLeaderboardData(); //Load leaderboard data on startup
        }
        else
        {
            Debug.Log("Request failed");
        }
    }

    /// <summary>
    /// Updates the chosenWorldID based on the dropdown menu option selected
    /// </summary>
    /// <param name="dropdownMenu">The dropdown menu to add the listener to</param>
    private void DropdownItemSelected(TMP_Dropdown dropdownMenu)
    {
        if (dropdownMenu.value == 0) //If "ALL WORLDS" option chosen
            chosenWorldID = 0;
        else
        {
            //-1 because worldOptions has one less entry than dropdownMenu due to "ALL WORLDS" being first option
            chosenWorldID = worldOptions[dropdownMenu.value-1].id; 
        }
        getLeaderboardData();
    }

    /// <summary>
    /// Retrieves leaderboard from backend and then calls displayLeaderboardData to display the data
    /// </summary>
    private void getLeaderboardData()
    {
        switch (chosenWorldID)
        {
            case 0: //For ALL WORLDS
                StartCoroutine(DataManager.getOverallLeaderboard(displayLeaderboardData));
                break;
            default:
                StartCoroutine(DataManager.getWorldLeaderboard(chosenWorldID.ToString(), displayLeaderboardData));
                break;
        }
    }

    /// <summary>
    /// Notify method called when request for World Leaderboard has been processed
    /// </summary>
    public void displayLeaderboardData()
    {
        if (DataManager.getRequestStatus() != true)
        {
            Debug.Log("Request Failed");
        }
        else
        {
            //success notif
            //Destroys all rows already present in the leaderboard
            foreach (Transform item in leaderboardPanel)
            {
                Destroy(item.gameObject);
            }

            //Displays rows based on current scores List<>
            foreach (Result i in LeaderboardData.leaderboard)
            {
                GameObject leaderboardRowObject = Instantiate(leaderboardRowPrefab, leaderboardPanel);
                TextMeshProUGUI[] texts = leaderboardRowObject.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = i.rank.ToString();
                texts[1].text = i.first_name + " " + i.last_name;
                texts[2].text = i.points.ToString();
                if (i.user_id == CurrentUser.loggedIn.id)
                {
                    foreach(var t in texts)
                    {
                        t.fontStyle = FontStyles.Bold;
                        t.color = new Color32(52, 235, 158, 255);
                    }
                }
                
            }
        }
    }

    /// <summary>
    /// Called when the share button is pressed
    /// Creates and opens a share link to the chosen social media
    /// </summary>
    public void share()
    {
        Debug.Log(chosenWorldID);
        Debug.Log(CurrentUser.loggedIn.id);

        if (chosenWorldID == 0)
        {
            Application.OpenURL("https://www.facebook.com/sharer/sharer.php?kid_directed_site=0&sdk=joey&u=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fpid%3D"
            + CurrentUser.loggedIn.id.ToString());
        }
        else
        {
            Application.OpenURL("https://www.facebook.com/sharer/sharer.php?kid_directed_site=0&sdk=joey&u=https%3A%2F%2Fcz3003.kado.sg%2Fshare%2Fhighscore%2F%3Fwid%3D"
            + chosenWorldID.ToString() + " %26pid%3D" + CurrentUser.loggedIn.id.ToString());
        }
    }
}
