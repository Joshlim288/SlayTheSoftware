using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI PositionObject;
    public TextMeshProUGUI TopicObject;
    public TextMeshProUGUI CurrentScoreObject;
    public TextMeshProUGUI TotalScoreObject;
    public TextMeshProUGUI DifficultyObject;
    private string PositionText;

    public void Update()
    {
        PositionText = "";
        if (CurrentUser.getChosenMap() <= 2)
        PositionText += "World " + (CurrentUser.getChosenMap() + 1).ToString() + " - ";
        
        PositionText += "Section " + (CurrentUser.getCurrentSection() + 1).ToString();

        PositionObject.text = PositionText;

        TopicObject.text = CurrentUser.getCurrentTopic();

        CurrentScoreObject.text = CurrentUser.getPlayerMapScore().ToString();

        TotalScoreObject.text = CurrentUser.getPlayerTotalScore().ToString();

        switch (CurrentUser.getDifficulty())
        {
            case(0):
                DifficultyObject.text = "Easy";
                break;
            case(1):
                DifficultyObject.text = "Medium";
                break;
            case(2):
                DifficultyObject.text = "Hard";
                break;
            default:
                DifficultyObject.text = "Error";
                break;
        }
    }
}
