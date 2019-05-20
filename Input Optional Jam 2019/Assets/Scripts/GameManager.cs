using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState {
    Setup,
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

    public GameObject ball = null;
    public GameObject spot;
    public List<Team> teams = new List<Team>();


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
        Kickoff();

    }


    public void ResetGame() {
        state = GameState.Setup;
        spot = Instantiate(spotPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity);

        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, Color.blue));
        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, Color.red));

    }

    public void Kickoff()
    {
        ball = Instantiate(ballPrefab, new Vector3(Random.Range(-10f, 10f), 25f, Random.Range(-10f, 10f)), Quaternion.identity);
        ball.GetComponent<BallBehavior>().SetOwner(0, null);
        state = GameState.Playing;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Team t in teams) {
            t.processAI();
        }

        /*if(Random.Range(0.0f, 1.0f) < newItemChance)
        {
            SpawnItem();
        }*/
    }

    public int GetBallOwner()
    {
        return ball.GetComponent<BallBehavior>().GetOwnerTeam();
    }

    public GameObject GetBallPlayer()
    {
        return ball.GetComponent<BallBehavior>().GetOwnerPlayer();
    }
}
