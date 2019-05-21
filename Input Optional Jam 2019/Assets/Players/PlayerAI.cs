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
        if (IsOnGround())
        {
            transform.LookAt(GameManager.Instance.ball.transform.position);
            GetComponent<Rigidbody>().AddForce(transform.forward * thrust, ForceMode.Impulse);
        }

    }

    void RunToGoal()
    {
        if (IsOnGround())
        {
            transform.LookAt(GameManager.Instance.spot.transform.position);
            GetComponent<Rigidbody>().AddForce(transform.forward * thrust, ForceMode.Impulse);
        }

    }


    void RunToOpenArea()
    {
        // TODO: This is probably a real bad way to do this
        GameObject p = FindNearestEnemy();
        transform.LookAt(2*transform.position - p.transform.position);
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust, ForceMode.Impulse);
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
                    if(d < distance) {
                        distance = d;
                        closest = player;
                    }
                }
            }
        }
        Debug.Log("dist: " + distance);
        return closest;
    }

    public void SetDebugText(string t)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = t;
    }
}
