using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GoalIndicator : MonoBehaviour
{
    public AnimationCurve showCurve;
    public float showLength = 6f;
    private float showTime;
    private bool active;
    private Image background;
    private TextMeshProUGUI text;
    private TextMeshProUGUI playerName;
    private TextMeshProUGUI teamName;

    private void Awake()
    {
        showTime = 0f; 
        active   = false;

        background = this.transform.Find("Background").GetComponent<Image>();
        text = this.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        playerName = this.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        teamName = this.transform.Find("TeamName").GetComponent<TextMeshProUGUI>(); 
    }

    public void ShowScore(string scoringPlayer, string scoringTeam, int score)
    {
        active   = true;
        showTime = 0f;

        playerName.text = scoringPlayer.ToUpper();
        teamName.text = scoringTeam.ToUpper(); 
    }

    //Cheap way to track the timer
    public void Update()
    {
        if (active)
        {
            text.gameObject.SetActive(showTime % 2 > 0.5f); 
            showTime += Time.deltaTime;

            background.color = new Color(background.color.r, 
                background.color.g, 
                background.color.b, 
                showCurve.Evaluate(showTime/showLength));

            text.color = new Color(text.color.r,
                text.color.g,
                text.color.b,
                showCurve.Evaluate(showTime/showLength));

            playerName.color = new Color(playerName.color.r,
                playerName.color.g,
                playerName.color.b,
                showCurve.Evaluate(showTime / showLength));

            teamName.color = new Color(teamName.color.r,
                teamName.color.g,
                teamName.color.b,
                showCurve.Evaluate(showTime / showLength));

            if (showTime > showLength) active = false; 
        }
    }
}
