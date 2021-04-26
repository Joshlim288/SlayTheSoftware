using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine.SceneManagement;

public class fbHolder : MonoBehaviour 
{
    public GameObject usernameF;
    private void Awake() 
    {
        FB.Init(SetInit,onHideUnity);
    }
    void SetInit() 
    {
        if(FB.IsLoggedIn)
        {
            Debug.Log("Logged in Successfully!");
        }
        else 
        Debug.Log("FB is not logged in");
    }
    void onHideUnity(bool isGameShown)
    {
        if (isGameShown)
        {
            Time.timeScale = 1;
        }
        else 
            Time.timeScale = 0;
    }
    public void fbLogin() 
    {
        // List<string> permissions = new List<string>();
        // permissions.Add("public_profile");
        // FB.LogInWithReadPermissions(permissions, AuthCallResult);
        var perms = new List<string>(){"public_profile", "email"};
        FB.LogInWithReadPermissions(perms, AuthCallResult);

    }
    void AuthCallResult(ILoginResult result)
    {
        if(result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else 
        {
            if(FB.IsLoggedIn)
            {
                Debug.Log("FB logged in");
                // Debug.Log(result.RawResult);
                FB.API("/me?fields=first_name", HttpMethod.GET, callbackData);
            }
            else 
            {
                Debug.Log("Login failed!");
            }
        }
    }
    void callbackData(IResult res)
    {
        Text username = usernameF.GetComponent<Text>();
        if(res.Error != null)
        {
            Debug.Log("Error getting data");

        }
        else 
        {
            username.text = "Welcome back, " +res.ResultDictionary["first_name"];
        }
    }

    public void SharePost() 
    {
        FB.FeedShare
        (
            // linkCaption:"Your Game Name!",
            
            linkName:"Hey Guys, Join me on Slay the Spire!",
            callback: null
            
        );
    }

    public void InviteFriends() 
    {
        FB.AppRequest("Let's play together!", null, null, null, null, null, "Slay the Software", delegate(IAppRequestResult result)
        {
            Debug.Log(result.RawResult);
        
        });
    }


    public void goBack()
    {
        if (CurrentUser.leaderorchallenge == 1) SceneManager.LoadScene("ViewChallenges");
        else SceneManager.LoadScene("Leaderboard");
    }
}
