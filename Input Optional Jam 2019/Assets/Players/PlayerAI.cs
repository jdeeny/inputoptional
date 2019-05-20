using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCommand
{
    Idle,
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
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.StartsWith("Spot"))
        {
            Debug.Log("Hit Spot");
            Debug.Log("Player: " + GameManager.Instance.GetBallPlayer());
            Debug.Log("this obj: " + gameObject);

            if(GameManager.Instance.GetBallPlayer() == gameObject) {
                Debug.Log("Hit Spot and have ball");

                GameManager.Instance.ball.GetComponent<BallBehavior>().Detach();
                Destroy(gameObject);
            

                GameManager.Instance.ball.GetComponent<Rigidbody>().AddExplosionForce(1700f, GameManager.Instance.spot.transform.position, 120f, 3.0F);
                foreach (Team t in GameManager.Instance.teams)
                {
                    foreach (GameObject player in t.players)
                    {
                        if (player != null)
                        {
                            Rigidbody rb = player.GetComponent<Rigidbody>();

                            if (rb != null)
                                rb.AddExplosionForce(500f, GameManager.Instance.spot.transform.position, 120f, 3.0F);
                        }
                    }

                }
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


    public bool IsOnGround()
    {
        return transform.position.y <= 0.5f;
    }

    public void SetCommand(PlayerCommand command)
    {
        current_command = command;
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
}
