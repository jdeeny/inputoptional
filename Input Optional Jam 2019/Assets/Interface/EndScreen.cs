using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class EndScreen : MonoBehaviour
{
    public float endScreenLength = 60f;
    private float endScreenTime = 0f;
    private bool onEndScreen = false;

    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI rebootText;

    public GameObject winnerSection; 
    public TextMeshProUGUI winnerText;

    public GameObject loserSection; 
    public TextMeshProUGUI loserText;

    public void ShowEndScreen()
    {
        //If we're already here don't show it again.
        if (onEndScreen) return; 

        onEndScreen = true;
        endScreenTime = 0f;

        //Parse the teams and set the winners and losers
        List<int> placement = new List<int>();  

        for (int i = 0; i < GameManager.Instance.teams.Count; ++i)
        {
            //Crappy way to do this, but just check against the existing array, then insert based on score.
            int placePos = 0; 
            foreach (int j in placement)
            {
                // Skip if it's the same team
                if (i == placement[j]) break;
                // >= to just skip tiebreakers
                if (GameManager.Instance.teams[placement[j]].teamScore >= GameManager.Instance.teams[i].teamScore)
                    placePos = i + 1; 
            }
            if (placePos >= placement.Count) { placement.Add(i); }
            else { placement.Insert(placePos, i); }
        }

        winnerText.text = GameManager.Instance.teams[placement[0]].teamName.ToUpper() + " -- " + 
            GameManager.Instance.teams[placement[0]].teamScore;
        loserText.text = "";
        for (int i = 1; i < placement.Count; ++i)
        {
            loserText.text += GameManager.Instance.teams[placement[i]].teamName.ToUpper() + " -- " + 
                GameManager.Instance.teams[placement[i]].teamScore + "\n"; 
        }
    }

    private void Update()
    {
        if (onEndScreen)
        {
            endScreenTime += Time.deltaTime;

            //Blinking end screen text
            gameOverText.gameObject.SetActive(endScreenTime % 2 > 0.5);

            //Update the reset text
            rebootText.text = "REBOOTING\n" + (endScreenLength - endScreenTime).ToString("0.0");

            if (endScreenTime > endScreenLength || Input.anyKeyDown)
            {
                onEndScreen = false;
                InterfaceHandler.instance.EndScreenOver();
            }
        }
    }
}
