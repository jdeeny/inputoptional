using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCommand
{
    Idle,
    GetOpen,
    CoverReceiver,
    GetBall,
    RunToGoal,
    Protect,
    Pass,
    Hit,
    Intercept,
}

public class PlayerAI : MonoBehaviour
{

    public float thrust;
    public float turn_speed;
    public float reaction_base;
    public float reaction_random;

    float reaction_remaining = 0;

    PlayerCommand current_command = PlayerCommand.Idle;
    int team = 0;

    int n = 0;
    // Update is called once per frame
    void Update()
    {
        reaction_remaining -= Time.deltaTime;
        if(reaction_remaining > 0f)
            return;

        switch (current_command)
        {
            default:
                StopMoving();
                break;
            case PlayerCommand.GetBall:
                RunToBall();
                break;
            case PlayerCommand.RunToGoal:
                RunToGoal();
                break;
            case PlayerCommand.GetOpen:
                RunToOpenArea();
                break;
            case PlayerCommand.Protect:
                RunTo(GetBallCarrierLocation());
                break;
            case PlayerCommand.Pass:
                PassTo(new Vector3(0f, 0f, 0f));
                break;
        }

    }

    void RunTo(Vector3 location)
    {
        if (IsOnGround())
        {
            transform.LookAt(location);
            Vector3 dir = transform.forward.normalized;
            dir.y = 0;
            GetComponent<Rigidbody>().AddForce(dir * thrust, ForceMode.Impulse);
        }
    }
    void RunAwayFrom(Vector3 location)
    {
        if (IsOnGround())
        {
            transform.LookAt(location);
            transform.Rotate(new Vector3(0f, 180f, 0f));
            Vector3 dir = transform.forward.normalized;
            dir.y = 0;
            GetComponent<Rigidbody>().AddForce(dir * thrust, ForceMode.Impulse);
        }
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.StartsWith("Spot"))
        {
            if(GameManager.Instance.GetBallPlayer() == gameObject) {
                Debug.Log("Hit Spot and have ball");

                GameManager.Instance.ball.GetComponent<BallBehavior>().Detach();
                GameManager.Instance.Score(GameManager.Instance.GetBallOwner());

                foreach (Team t in GameManager.Instance.teams)
                {
                    foreach (GameObject player in t.players)
                    {
                        if (player != null)
                        {
                            Rigidbody rb = player.GetComponent<Rigidbody>();

                            if (rb != null)
                                rb.AddExplosionForce(1000f, GameManager.Instance.spot.transform.position, 120f, 3.0F);
                        }
                    }

                }
                GameManager.Instance.teams[team - 1].RemovePlayer(gameObject);
                Destroy(gameObject,0.1f);
            }
        }
    }

    void StopMoving()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (IsOnGround())
        {
            if (body.velocity.magnitude >= 0.001f)
            {
                //body.velocity = Vector3.zero;
                body.AddForce(body.velocity.normalized * -0.1f * thrust, ForceMode.Impulse);
            }
        }
    }

    void RunToBall()
    {
        RunTo(GameManager.Instance.ball.transform.position);
    }

    void RunToGoal()
    {
        RunTo(GameManager.Instance.spot.transform.position);
    }


    void RunToOpenArea()
    {
        // TODO: This is probably a real bad way to do this
        GameObject p = FindNearestPlayer();
        RunAwayFrom(transform.position - p.transform.position);
    }

    public bool IsOnGround()
    {
        return transform.position.y <= 0.5f;
    }

    public void SetCommand(PlayerCommand command)
    {
        current_command = command;
        SetDebugText(command.ToString());
        reaction_remaining = reaction_base + Random.Range(0f, 1f) * reaction_random;
    }

    public void SetTeam(int team_num)
    {
        team = team_num;
    }

    public int GetTeam()
    {
        return team;
    }

    public GameObject FindNearestEnemy() {
        GameObject closest = null;
        float distance = 10000000f;
        foreach(Team t in GameManager.Instance.teams) {
            if(t.teamNumber != team) {
                foreach(GameObject player in t.players) {
                    float d = Vector3.Distance(player.transform.position, transform.position);
                    if(d >= 1 && d < distance) {
                        distance = d;
                        closest = player;
                    }
                }
            }
        }
        return closest;
    }

    public GameObject FindNearestPlayer()
    {
        GameObject closest = null;
        float distance = 10000000f;
        foreach (Team t in GameManager.Instance.teams)
        {
            foreach (GameObject player in t.players)
            {
                if (player != this)
                {
                    float d = Vector3.Distance(player.transform.position, transform.position);
                    if (d >= 1 && d < distance)
                    {
                        distance = d;
                        closest = player;
                    }
                }
            }
            
        }
        return closest;
    }


    public void SetDebugText(string t)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = t;
    }


    Vector3 GetBallCarrierLocation()
    {
        return GameManager.Instance.GetBallPlayer().transform.position;
    }


    void PassTo(Vector3 location)
    {
        GameManager.Instance.ball.GetComponent<BallBehavior>().ThrowTo(location);
    }

}
