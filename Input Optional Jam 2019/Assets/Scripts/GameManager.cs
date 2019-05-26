using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
    Setup,
    PreKickoff,
    Playing,
    GameOver,
}

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private AudioClip[] kickoffClips;
    private AudioSource kickoffSource;

    public static GameManager Instance { get; set; }

    public GameObject playerPrefab;
    public GameObject spotPrefab;
    public GameObject ballPrefab;
    public GameObject boundsPrefab;

    public int playToScore; //Ideally the same as playersPerTeam 
    public int playersPerTeam; 
    public int maxTeams;

    public GameObject ball = null;
    public GameObject spot;
    public List<Team> teams = new List<Team>();

    float delay = 0f;

    public Scene mainScene;
    public Scene hiddenScene;
    GameObject hiddenBounds;
    GameObject hiddenBall;

    float timeSinceLastHiddenSim = 0f;
    float timeBetweenHiddenSim = 0.2f;
    GameState state;

    public float newPlayerChance;

    //public float ballTimeToGround = 0f;
    //public Vector3 ballLandingPosition = Vector3.zero;

    public List<Vector3> ballPositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            kickoffSource = GetComponent<AudioSource>();
        } else
        {
            Debug.Log("Only one of this object is permitted");
            Destroy(gameObject);
            return;
        }

        ResetGame();
        CreateHiddenScene();
    }


    void CreateHiddenScene()
    {
        Physics.autoSimulation = false;
        mainScene = SceneManager.GetActiveScene();
        hiddenScene = SceneManager.CreateScene("Hidden Scene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));

        SceneManager.SetActiveScene(hiddenScene);

        hiddenBounds = GameObject.Instantiate(boundsPrefab, Vector3.zero, Quaternion.identity);
        hiddenBounds.name = "hiddenBounds";
        hiddenBall = GameObject.Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        hiddenBall.name = "hiddenBall";
        Destroy(hiddenBall.GetComponentInChildren<MeshRenderer>());
        Destroy(hiddenBall.GetComponentInChildren<Light>());
        Destroy(hiddenBall.GetComponentInChildren<BallBehavior>());
        SceneManager.SetActiveScene(mainScene);
    }

    void SimulateHiddenScene()
    {
        timeSinceLastHiddenSim = 0f;
        hiddenBall.transform.position = ball.transform.position;
        hiddenBall.transform.rotation = ball.transform.rotation;
        hiddenBall.GetComponent<Rigidbody>().velocity = ball.GetComponent<Rigidbody>().velocity;
        hiddenBall.GetComponent<Rigidbody>().angularVelocity = ball.GetComponent<Rigidbody>().angularVelocity;
        hiddenBall.GetComponent<Rigidbody>().inertiaTensor = ball.GetComponent<Rigidbody>().inertiaTensor;
        hiddenBall.GetComponent<Rigidbody>().inertiaTensorRotation = ball.GetComponent<Rigidbody>().inertiaTensorRotation;

        float timeScale = 2f;
        float timeToGround = 0f;

        List<Vector3> posns = new List<Vector3>();
        do
        {
            hiddenScene.GetPhysicsScene().Simulate(Time.fixedDeltaTime * timeScale);
            timeToGround += Time.fixedDeltaTime * timeScale;
            posns.Add(hiddenBall.transform.position);
        } while (timeToGround < 6f);
        ballPositions = posns;
    }

    public void ResetGame() {
        state = GameState.Setup;
        spot = Instantiate(spotPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity);
        ball = Instantiate(ballPrefab, new Vector3(Random.Range(-10f, 10f), 5f, Random.Range(-10f, 10f)), Quaternion.identity);

        if (teams.Count == 0)
        {
            Debug.Log("StartTeam");
            teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 1));
            teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 2));
            teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, 3));
        } else
        {
            Debug.Log("RefreshTeam");
            foreach (Team team in teams)
            {
                team.resetSelf(playerPrefab, playersPerTeam); 
            }
        }
        timeSinceLastHiddenSim = 0f;

        if (InterfaceHandler.instance != null) InterfaceHandler.instance.SetupTeams(); 

        ReadyKickoff();
    }

    public void DoKickoff()
    {
        ball.GetComponent<BallBehavior>().SetOwner(0, null);
        PlayKickoffHorn();
        ball.GetComponent<BallBehavior>().DoKickoff();
        state = GameState.Playing;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Do the normal physics update
        mainScene.GetPhysicsScene().Simulate(Time.fixedDeltaTime);

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
                timeSinceLastHiddenSim += Time.fixedDeltaTime;
                if (timeSinceLastHiddenSim > timeBetweenHiddenSim)
                    SimulateHiddenScene();

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
        foreach(var t in teams)
            t.ReadyKickoff();
        delay = 3f;
        state = GameState.PreKickoff;
    }

    public void Score(int team, string playerName) {
        teams[team].teamScore++; 

        InterfaceHandler.instance.ShowGoalBanner(team, playerName, teams[team].teamScore);
        InterfaceHandler.instance.UpdateTeamScores(); 

        if (teams[team].teamScore >= playToScore)
        {
            Debug.Log("ShowEndScreen");
            InterfaceHandler.instance.ShowEndScreen(); 
        }
        else
        {
            Debug.Log("Kickoff");
            ReadyKickoff();
        }
    }

    public bool IsPreKickoff() {
        return state == GameState.PreKickoff;
    }

    private void PlayKickoffHorn()
    {
        kickoffSource.PlayOneShot(kickoffClips[Random.Range(0, kickoffClips.Length)]);
    }

}
