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

    public GameObject ingameUI;
    public GameObject endscreenUI; 

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
        goalIndicator.ShowScore(playerName, GameManager.Instance.teams[team-1].teamName, 0); 
    }

    public void ShowEndScreen()
    {
        ingameUI.SetActive(false); 
        endscreenUI.SetActive(true);
        endscreenUI.GetComponent<EndScreen>().ShowEndScreen();
    }

    public void EndScreenOver()
    {
        ingameUI.SetActive(true); 
        endscreenUI.SetActive(false);
        GameManager.Instance.ResetGame(); 
    }
}
