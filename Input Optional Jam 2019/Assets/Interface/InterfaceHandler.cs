using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceHandler : MonoBehaviour
{
    public float endScreenLength = 60f;
    private float endScreenTime  = 0f;
    private bool onEndScreen     = false; 

    public static InterfaceHandler instance
    {
        get
        {
            return _instance; 
        }
    }
    private static InterfaceHandler _instance;
    private TeamIndicator[] teamIndicators;
    private GoalIndicator goalIndicator; 

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("Cannot have more than one interface handler."); 
        } else
        {
            _instance = this; 
        }

        teamIndicators = GetComponentsInChildren<TeamIndicator>();
        goalIndicator  = GetComponentInChildren<GoalIndicator>();
    }

    public void SetupTeams()
    {
        //Set the UI elements to start off 
        for (int i = 0; i < GameManager.Instance.teams.Count && i < teamIndicators.Length; ++i)
        {
            teamIndicators[i].UpdateName(GameManager.Instance.teams[i].teamName);
            teamIndicators[i].UpdateScore(GameManager.Instance.teams[i].teamScore);
            teamIndicators[i].SetColor(GameManager.Instance.teams[i].teamColor);
        }
    }

    public void UpdateTeamScores()
    {
        //Make the scores for individual teams update
        for (int i = 0; i < GameManager.Instance.teams.Count && i < teamIndicators.Length; ++i)
        {
            teamIndicators[i].UpdateScore(GameManager.Instance.teams[i].teamScore);
        }
    }

    public void ShowGoalBanner(int team, string playerName, int score)
    {
        //Make the goal banner appear temporarily, play sounds
        //Need to get the scoring team and player?
        goalIndicator.ShowScore(playerName, GameManager.Instance.teams[team].teamName, 0); 
    }

    public void ShowEndScreen()
    {
        //Show the screen at the end of the game

        //Wait however long then reset the game
        onEndScreen   = true;
        endScreenTime = 0f;
    }

    //Use for any timers like the end screen
    private void Update()
    {
        if (onEndScreen)
        {
            endScreenTime += Time.deltaTime; 

            if (endScreenTime > endScreenLength)
            {
                onEndScreen = false;
                GameManager.Instance.ResetGame(); 
            }
        }
    }
}
