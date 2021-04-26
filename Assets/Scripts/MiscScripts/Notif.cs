using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// Class that defines a notification popup
/// </summary>
public class Notif : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] TextMeshProUGUI _buttonText;
    [SerializeField] TextMeshProUGUI _popupText;

    /// <summary>
    /// Initialisation function to pass in required parameters
    /// </summary>
    /// <param name="canvas">canvas The number to raise</param>
    /// <param name="popupMessage">The message to be placed in the pop-up</param>
    /// <param name="btntxt">The text to be placed in the button</param>
    /// <param name="action">The action to be taken when the right button is pressed.</param>
    public void Init(Transform canvas, string popupMessage, string btntxt, Action action)
    {
        _popupText.text = popupMessage;
        _buttonText.text = btntxt;

        //Set canvas to initialise pop-up on
        transform.SetParent(canvas);
        transform.localScale = Vector3.one;
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = Vector2.zero;

        //Clicking on buton will execute the action passed in
        _button.onClick.AddListener(() =>
        {
            action();
            GameObject.Destroy(this.gameObject);
        });
    }

    /// <summary>
    /// Instantiates and returns a Popup game object
    /// </summary>
    public static Notif CreateNotif()
    {
        GameObject notifGameObject = Instantiate(Resources.Load("Notif") as GameObject);
        return notifGameObject.GetComponent<Notif>();
    }

}
