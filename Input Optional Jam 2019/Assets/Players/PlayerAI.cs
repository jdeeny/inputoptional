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
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.StartsWith("Spot"))
        {
            Destroy(gameObject);
            foreach (Team t in GameManager.Instance.teams)
            {
                foreach (GameObject player in t.players)
                {
                    if (player != null)
                    {
                        Rigidbody rb = player.GetComponent<Rigidbody>();

                        if (rb != null)
                            rb.AddExplosionForce(1700f, GameManager.Instance.spot.transform.position, 120f, 3.0F);
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
                Debug.Log("Trying To Stop" + n++);
                //body.velocity = Vector3.zero;
                body.AddForce(body.velocity.normalized * -0.1f * thrust, ForceMode.Impulse);
            }
        }
    }

    void RunToBall()
    {
        if (IsOnGround())
        {
            Debug.Log("Trying To Get Ball");
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
