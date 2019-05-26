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
    Pass,
    Intercept,
}

public class Team : ScriptableObject
{
    public List<GameObject> players = new List<GameObject>();

    public int teamNumber = 1;

    float passChance = 0.001f;

    float redoAiTime = 0f;

    float decisionTimeout = 0f;

    public TeamGoal teamGoal = TeamGoal.Nothing;

    //Placeholder values for UI/Score/Etc.
    public string teamName;

    public Color teamColor = new Color(0, 0, 0);

    public int teamScore = 0; 

    public void Init(GameObject playerPrefab, int players)
    {
        for (int i = 0; i < players; i++)
        {
            addPlayer(playerPrefab, teamColor, i);
        }
    }

    public static Team CreateInstance(GameObject prefab, int players, int number)
    {
        var team = ScriptableObject.CreateInstance<Team>();
        team.teamNumber = number;
        team.teamName = NameGenerator.GenerateCityName();
        team.teamColor = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
        team.Init(prefab, players);
        return team;
    }

    public void resetSelf(GameObject playerPrefab, int players)
    {
        teamScore = 0;
        teamName = NameGenerator.GenerateCityName();
        teamColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        for (int i = 0; i < players; i++)
        {
            addPlayer(playerPrefab, teamColor, i);
        }
    }

    void addPlayer(GameObject playerPrefab, Color c, int playerNumber)
    {
        Vector2 circ;
        do {
            circ = Random.insideUnitCircle * 30f;
        } while(Mathf.Abs(circ.x) > 5f && Mathf.Abs(circ.y) > 5f);
        Vector3 loc = new Vector3(circ.x, 1.5f, circ.y);
        GameObject new_player = Instantiate(playerPrefab, loc, Quaternion.identity);
        new_player.GetComponent<PlayerAI>().SetTeam(teamNumber);
        var renderers = new_player.transform.GetComponentsInChildren<Renderer>();
        foreach(var r in renderers) {
            if (r.gameObject.name == "RobbitBody") {
               r.materials[0].color = c;
            }
        }
        //new_player.transform.GetComponentInChildren<Renderer>().material.color = c;
        new_player.name = "Player " + teamNumber + "-" + playerNumber;
        players.Add(new_player);
    }

    public void processAI()
    {
        decisionTimeout -= Time.deltaTime;

        if (decisionTimeout <= 0f && decideTeamGoal())
        {
            decisionTimeout = 0.2f;
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
                case TeamGoal.Pass:
                    commandPass();
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
            if(Random.Range(0f,1f) < passChance)
            {
                teamGoal = TeamGoal.Pass;
            } else
            {
                teamGoal = TeamGoal.ScoreGoal;
            }
        } else
        {
            teamGoal = TeamGoal.ForceFumble;
        }

        if (teamGoal != oldGoal) {
            //Debug.Log("New Goal");
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
                //Debug.Log("Told player to go to goal");
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.RunToGoal);
            }
            else
            {
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.Protect);
            }
        }

    }

    public void ReadyKickoff()
    {
        foreach(var p in players) {
            p.GetComponent<PlayerAI>().ReadyKickoff();
        }
    }
    void commandForceFumble()
    {
        commandNothing();
    }

    void commandPreKickoff()
    {
        //Debug.Log("Told players to get open");

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

    void commandPass()
    {
        GameObject ball_holder = GameManager.Instance.GetBallPlayer();

        if (ball_holder == null)
        {
            return;
        }
        foreach (GameObject p in players)
        {
            if (p == ball_holder)
            {
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.Pass);
            }
            else
            {
                p.GetComponent<PlayerAI>().SetCommand(PlayerCommand.GetOpen);
            }
        }

    }

}
