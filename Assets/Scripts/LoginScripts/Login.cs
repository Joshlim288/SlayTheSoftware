using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Script for handling Login and Register functionality
/// </summary>
public class Login : MonoBehaviour
{
    public Transform MainCanvas;
    public GameObject loginMenu;
    public GameObject registerMenu;
    public InputField usernameInput;
    public InputField passwordInput;
    public GameObject invalidPrompt;
    public GameObject invalidRegisterPrompt;
    private string username;
    private string password;
    public InputField newPasswordInput;
    public InputField reenterNewPasswordInput;

    /// <summary>
    /// Called when the login button is pressed
    /// Retrieves text in the Username and Password fields and sends to server for checking
    /// </summary>
    public void VerifyLogin ()
    {
        username = usernameInput.text;
        password = passwordInput.text;

        StartCoroutine(DataManager.login(username, password, loginNotify));
    }
    /// <summary>
    /// Notfy method to be called when login request is processed
    /// Checks whether login was valid, invalid or a first time login
    /// </summary>
    public void loginNotify()
    {
        if (DataManager.getRequestStatus() != true)
        {
            invalidPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            invalidPrompt.SetActive(true);
        }
        else if (DataManager.token == null)
        {

            Action action = () => {
                loginMenu.SetActive(false);
                registerMenu.SetActive(true);
            };

            Notif changeNotif = Notif.CreateNotif();

            //Initialise the notif
            changeNotif.Init(MainCanvas,
                       "First-time login, please proceed to reset password!",
                       "Okay",
                       action
                      );
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Called when the reset password button is clicked 
    /// Retrieves new password and reentered new password and sends them to the server for verification
    /// </summary>
    public void VerifyRegister()
    {
        string newPassword = newPasswordInput.text;
        Debug.Log(newPassword);
        string reenterNewPassword = reenterNewPasswordInput.text;
        Debug.Log(reenterNewPassword);
        TextMeshProUGUI message = invalidRegisterPrompt.GetComponent<TextMeshProUGUI>();

        if (newPassword == "" || reenterNewPassword == "")
        {
            message.text = "Password field is empty!";
            invalidRegisterPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            invalidRegisterPrompt.SetActive(true);
        }
        else if(newPassword != reenterNewPassword)
        {
            message.text = "Passwords do not match!";
            invalidRegisterPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            invalidRegisterPrompt.SetActive(true);
        }
        else
        {
            StartCoroutine(DataManager.changePassword(username, password, newPassword, changePasswordNotify));
        }
    }

    /// <summary>
    /// Notify method to be called when reset password request is processed
    /// Checks whether password has been changed successfully, or if there were any requirements not met
    /// </summary>
    public void changePasswordNotify()
    {
        TextMeshProUGUI message = invalidRegisterPrompt.GetComponent<TextMeshProUGUI>();
        if (DataManager.getRequestStatus() == true)
        {
            Debug.Log("Success");
            //Pop-up saying registered

            Action action = () => {
                loginMenu.SetActive(true);
                registerMenu.SetActive(false);
                invalidPrompt.SetActive(false);
                passwordInput.text = "";

            };

            Notif successNotif = Notif.CreateNotif();

            //Initialise the notif
            successNotif.Init(MainCanvas,
                       "Password has been changed successfully!",
                       "Okay",
                       action
                      );

        }
        else
        {
            invalidRegisterPrompt.SetActive(false); //To restart the animation if invalid prompt is already active
            message.text = CurrentUser.errorMessage;
            invalidRegisterPrompt.SetActive(true);
        }
    }
}
