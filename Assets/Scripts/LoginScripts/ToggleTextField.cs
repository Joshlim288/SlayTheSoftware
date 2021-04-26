using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Utility script for Text Entry
/// Allows for moving between fields with tab, shift+tab and to submit with return
/// </summary>
public class ToggleTextField : MonoBehaviour
{
    EventSystem system;
    public Selectable firstInput;
    public Button submitButton;

    /// <summary>
    /// Called when the scene is first loaded
    /// Initializes to first input being selected
    /// </summary>
    void Start()
    {
        system = EventSystem.current;
        firstInput.Select();
    }

    /// <summary>
    /// Called once every frame
    /// Detect button presses on Tab, LeftShift and Return
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            Selectable previous = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (previous != null)
            {
                previous.Select();
            }
        }

        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                next.Select();
            }
        }

        else if (Input.GetKeyDown(KeyCode.Return))
        {
            submitButton.onClick.Invoke();
            Debug.Log("Button pressed!");
        }
    }
}
