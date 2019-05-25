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

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("Cannot have more than one interface handler."); 
        } else
        {
            _instance = this; 
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            TeamIndicator[] teams = this.GetComponentsInChildren<TeamIndicator>();
            foreach (TeamIndicator team in teams)
            {
                team.UpdateName(NameGenerator.GenerateCityName());
                team.UpdateScore(Random.Range(0, 5));
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.LogWarning(NameGenerator.GenerateRobotName());
        }
    }

    private void SetupTeams()
    {
        //Set the UI elements to start off
    }

    public void UpdateTeamScores()
    {
        //Make the scores for individual teams update
    }

    public void ShowGoalBanner()
    {
        //Make the goal banner appear temporarily, play sounds
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
