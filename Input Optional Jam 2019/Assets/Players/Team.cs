using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamGoal
{
    Nothing,
    PreKickoff,
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
            addPlayer(playerPrefab, c);
        }
    }

    public static Team CreateInstance(GameObject prefab, int players, int number, Color color)
    {
        var team = ScriptableObject.CreateInstance<Team>();
        team.teamNumber = number;
        team.Init(prefab, players, color);
        return team;
    }

    void addPlayer(GameObject playerPrefab, Color c)
    {
        GameObject new_player = Instantiate(playerPrefab, new Vector3(Random.Range(-50.0f, 50.0f), 4f, Random.Range(-50.0f, 50.0f)), Quaternion.identity);
        new_player.GetComponent<PlayerAI>().SetTeam(teamNumber);
        new_player.transform.GetComponentInChildren<Renderer>().material.color = c;
        players.Add(new_player);
    }

    public void processAI()
    {
        if (decideTeamGoal() || Random.Range(0f,1f) < 0.05)
        {
            switch(teamGoal)
            {
                case TeamGoal.PickupBall:
                    commandPickupBall();
                    break;
                case TeamGoal.ScoreGoal:
                    commandScoreGoal();
                    break;
                case TeamGoal.ForceFumble:
                    commandForceFumble();
                    break;
                case TeamGoal.PreKickoff:
                    commandPreKickoff();
                    break;
                default:
                    commandNothing();
                    break;
            }
        }
    }

    bool decideTeamGoal()
    {
        TeamGoal oldGoal = teamGoal;

        if(GameManager.Instance.IsPreKickoff()) {
            teamGoal = TeamGoal.PreKickoff;
        } else if(ballLoose())
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

    void commandNothing()
    {
        foreach (GameObject p in players)
        {
            p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.Idle);
        }
    }

    void commandPickupBall()
    {
        foreach (GameObject p in players)
        {
            p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.GetBall);
        }
    }

    void commandScoreGoal()
    {
        GameObject ball_holder = GameManager.Instance.GetBallPlayer();

        if(ball_holder == null)
        {
            return;
        }
        foreach (GameObject p in players)
        {
            if (p == ball_holder)
            {
                Debug.Log("Told player to go to goal");
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.RunToGoal);
            }
            else
            {
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.Protect);
            }
        }

    }

    void commandForceFumble()
    {

    }

    void commandPreKickoff()
    {
        Debug.Log("Told players to get open");

        foreach (GameObject p in players)
        {
            p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.GetOpen);
        }
    }



    bool ballLoose()
    {
        return GameManager.Instance.GetBallOwner() == 0;
    }

    bool hasBall()
    {
        return GameManager.Instance.GetBallOwner() == teamNumber;
    }


    public void RemovePlayer(GameObject p) {
        players.Remove(p);
    }

}
