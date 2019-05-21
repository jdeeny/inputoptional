using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState {
    Setup,
    PreKickoff,
    Playing,
    GameOver,
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; set; }

    public GameObject playerPrefab;
    public GameObject spotPrefab;
    public GameObject ballPrefab;

    public int playersPerTeam;
    public int maxTeams;

    public GameObject ball = null;
    public GameObject spot;
    public List<Team> teams = new List<Team>();

    float delay = 0f;

    GameState state;

    public float newPlayerChance;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.Log("Only one of this object is permitted");
            Destroy(gameObject);
            return;
        }

        ResetGame();
    }


    public void ResetGame() {
        state = GameState.Setup;
        spot = Instantiate(spotPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity);
        ball = Instantiate(ballPrefab, new Vector3(Random.Range(-10f, 10f), 5f, Random.Range(-10f, 10f)), Quaternion.identity);

        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 1, Color.blue));
        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 2, Color.red));
        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 3, Color.green));
        ReadyKickoff();
    }

    public void DoKickoff()
    {
        ball.GetComponent<BallBehavior>().SetOwner(0, null);
        ball.GetComponent<BallBehavior>().DoKickoff();
        state = GameState.Playing;
    }

    // Update is called once per frame
    void Update()
    {

        switch(state) {
            case GameState.Setup:
                break;
            case GameState.PreKickoff:
                delay -= Time.deltaTime;
                ProcessTeamAI();
                if(delay <= 0) {
                    DoKickoff();
                }
                break;
            case GameState.Playing:
                ProcessTeamAI();
                break;
        }

        /*if(Random.Range(0.0f, 1.0f) < newItemChance)
        {
            SpawnItem();
        }*/
    }

    void ProcessTeamAI() {
        foreach (Team t in teams) {
            t.processAI();
        }

    }

    public int GetBallOwner()
    {
        return ball.GetComponent<BallBehavior>().GetOwnerTeam();
    }

    public GameObject GetBallPlayer()
    {
        return ball.GetComponent<BallBehavior>().GetOwnerPlayer();
    }

    public void ReadyKickoff() {
        ball.GetComponent<BallBehavior>().Reset();
        state = GameState.PreKickoff;
        delay = 3f;
    }

    public void Score(int team) {
        ReadyKickoff();
    }

    public bool IsPreKickoff() {
        return state == GameState.PreKickoff;
    }
}
