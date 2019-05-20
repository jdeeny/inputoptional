﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamGoal
{
    Nothing,
    PickupBall,
    ScoreGoal,
    ForceFumble,
    Intercept,
}

public class Team : ScriptableObject
{
    public List<GameObject> players = new List<GameObject>();

    public int teamNumber = 1;

    public TeamGoal teamGoal = TeamGoal.Nothing;

    public void Init(GameObject playerPrefab, int players, Color c)
    {
        for (int i = 0; i < players; i++)
        {
            addPlayer(playerPrefab);
        }
    }

    public static Team CreateInstance(GameObject prefab, int players, Color color)
    {
        var team = ScriptableObject.CreateInstance<Team>();
        team.Init(prefab, players, color);
        return team;
    }

    void addPlayer(GameObject playerPrefab)
    {
        GameObject new_player = Instantiate(playerPrefab, new Vector3(Random.Range(-50.0f, 50.0f), 0.5f, Random.Range(-50.0f, 50.0f)), Quaternion.identity);
        new_player.GetComponent<PlayerAI>().SetTeam(teamNumber);
        players.Add(new_player);
    }

    public void processAI()
    {
        decideTeamGoal();
    }

    bool decideTeamGoal()
    {
        TeamGoal oldGoal = teamGoal;

        if(ballLoose())
        {
            teamGoal = TeamGoal.PickupBall;
        } else if(hasBall())
        {
            teamGoal = TeamGoal.ScoreGoal;
        } else
        {
            teamGoal = TeamGoal.ForceFumble;
        }

        if (teamGoal != oldGoal) {
            Debug.Log("New Goal");
            return true;
        }
        return false;
    }

    bool ballLoose()
    {
        return GameManager.Instance.GetBallOwner() == 0;
    }

    bool hasBall()
    {
        return GameManager.Instance.GetBallOwner() == teamNumber;
    }


}