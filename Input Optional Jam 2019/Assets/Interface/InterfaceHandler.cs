using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceHandler : MonoBehaviour
{
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
        Debug.Log(GameManager.Instance.teams.Count);
        Debug.Log(teamIndicators.Length); 
        for (int i = 0; i < GameManager.Instance.teams.Count && i < teamIndicators.Length; ++i)
        {
            teamIndicators[i].UpdateName(GameManager.Instance.teams[i].teamName);
            teamIndicators[i].UpdateScore(GameManager.Instance.teams[i].teamScore);
        }
    }

    public void UpdateTeamScores()
    {
        //Make the scores for individual teams update
    }

    public void ShowGoalBanner()
    {
        //Make the goal banner appear temporarily, play sounds
        //Need to get the scoring team and player?
        goalIndicator.ShowScore("", "", 0); 
    }

    public void ShowEndScreen()
    {
        //Show the screen at the end of the game
    }

    public void BtnStartGame()
    {
        //UI hook
    }

    public void BtnShowCredits()
    {
        //UI hook
    }

    public void BtnExitGame()
    {
        //UI hook
    }
}
