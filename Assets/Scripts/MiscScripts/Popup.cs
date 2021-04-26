using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// Class that defines a Popup
/// </summary>
public class Popup : MonoBehaviour
{
    [SerializeField] Button _button1;
    [SerializeField] Button _button2;
    [SerializeField] TextMeshProUGUI _button1Text;
    [SerializeField] TextMeshProUGUI _button2Text;
    [SerializeField] TextMeshProUGUI _popupText;

     /// <summary>
     /// Initialisation function to pass in required parameters, left button only destroys pop-up
     /// </summary>
     /// <param name="canvas">canvas The number to raise.</param>
     /// <param name="popupMessage">The message to be placed in the pop-up</param>
     /// <param name="btn1txt">The text to be placed in the left button.</param>
     /// <param name="btn2txt">The text to be placed in the right button.</param>
     /// <param name="rightAction">The action to be taken when the right button is pressed.</param>
     /// <param name="leftAction">The action to be taken when the left button is pressed.</param>
    public void Init(Transform canvas, string popupMessage, string btn1txt, string btn2txt, Action rightAction, Action leftAction)
    {
        _popupText.text = popupMessage;
        _button1Text.text = btn1txt;
        _button2Text.text = btn2txt;

        //Set canvas to initialise pop-up on
        transform.SetParent(canvas);
        transform.localScale = Vector3.one;
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = Vector2.zero;

        //Clicking on left button will execute the action passed in
        _button1.onClick.AddListener(() =>
        {
            leftAction();
            GameObject.Destroy(this.gameObject);
        });

        //Clicking on right buton will execute the action passed in
        _button2.onClick.AddListener(() =>
        {
            rightAction();
            GameObject.Destroy(this.gameObject);
        });
    }

    /// <summary>
    /// Instantiates and returns a Popup game object
    /// </summary>
    /// <returns>Popup GameObject</returns>
    public static Popup CreatePopup()
    {
        GameObject popUpGameObject = Instantiate(Resources.Load("Popup") as GameObject);
        return popUpGameObject.GetComponent<Popup>();
    }

}
