using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

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
    float nearRadius = 5f;
    float visionRadius = 30f;

    float reaction_remaining = 0;

    int layer = 9;

    PlayerCommand current_command = PlayerCommand.Idle;
    int team = 0;

    int n = 0;

    Collider col;
    Rigidbody rb;
    Animator animator;

    Dictionary<string, HashSet<Collider>> visionSets;

    void Start()
    {
        col = transform.gameObject.GetComponent<Collider>();
        rb = transform.gameObject.GetComponent<Rigidbody>();
        animator = transform.gameObject.GetComponent<Animator>();
        gameObject.layer = LayerMask.NameToLayer("Player");
    }


    void FixedUpdate()
    {
        visionSets = UpdatePlayerVision();

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
                StopMoving();
                //RunTo(GetBallCarrierLocation());
                break;
            case PlayerCommand.Pass:
                StopMoving();
                //PassTo(new Vector3(0f, 0f, 0f));
                break;
        }

        // Keep upright
        float uprightTorque = 500f;
        var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
        rb.AddTorque(new Vector3(rot.x, rot.y, rot.z)*uprightTorque);

        float localVel = Vector3.Dot(transform.forward, rb.velocity);
        Debug.Log(localVel);
        animator.SetFloat("forward", localVel/ 10f);

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
        RunTo(GameManager.Instance.ballLandingPosition);
    }

    void RunToGoal()
    {
        RunTo(GameManager.Instance.spot.transform.position);
    }


    void RunToOpenArea()
    {
        Vector3 target = Vector3.zero;

        foreach(var c in visionSets["vision"])
        {
            target += c.transform.position;
        }
        target /= visionSets["vision"].Count;

        
        RunTo(target);
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




    Vector3 GetBallCarrierLocation()
    {
        return GameManager.Instance.GetBallPlayer().transform.position;
    }


    void PassTo(Vector3 location)
    {
        GameManager.Instance.ball.GetComponent<BallBehavior>().ThrowTo(location);
    }


    public Physics.PreviewCondition preview = Physics.PreviewCondition.Editor;
    public float drawDuration = 0;
    public Color hitColor = Color.green;
    public Color noHitColor = Color.red;

    Dictionary<string, HashSet<Collider>> UpdatePlayerVision()
    {
        Vector3 halfExtents = new Vector3(visionRadius, visionRadius, visionRadius);
        Vector3 frontOffset = new Vector3(visionRadius / 2f, 0f, 0f);
        Vector3 leftOffset = new Vector3(0f, 0f, visionRadius / 2f);
        Quaternion orientation = Quaternion.LookRotation(transform.forward, transform.up);

        LayerMask layerMaskPlayer = LayerMask.GetMask("Player");
        LayerMask layerMaskBall = LayerMask.GetMask("Ball");
        Dictionary<string, HashSet<Collider>> sets = new Dictionary<string, HashSet<Collider>>();

        sets["nearBall"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, nearRadius, layerMaskBall));//, preview, drawDuration, hitColor, noHitColor));
        sets["near"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, nearRadius, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));
        sets["vision"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, visionRadius, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));
        sets["leftcenter"] = new HashSet<Collider>(Physics.OverlapBox(transform.position + leftOffset, halfExtents, orientation, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));
        sets["rightcenter"] = new HashSet<Collider>(Physics.OverlapBox(transform.position - leftOffset, halfExtents, orientation, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));
        sets["frontmiddle"] = new HashSet<Collider>(Physics.OverlapBox(transform.position + frontOffset, halfExtents, orientation, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));
        sets["backmiddle"] = new HashSet<Collider>(Physics.OverlapBox(transform.position - frontOffset, halfExtents, orientation, layerMaskPlayer));//, preview, drawDuration, hitColor, noHitColor));

        // Remove myself
        foreach (KeyValuePair<string, HashSet<Collider>> kvp in sets)
        {
            kvp.Value.Remove(col);
        }

        sets["far"] = new HashSet<Collider>(sets["vision"]);
        sets["far"].ExceptWith(sets["near"]);

        sets["center"] =  new HashSet<Collider>(sets["leftcenter"]);
        sets["center"].IntersectWith(sets["rightcenter"]);

        sets["middle"] = new HashSet<Collider>(sets["frontmiddle"]);
        sets["middle"].IntersectWith(sets["backmiddle"]);

        sets["left"] = new HashSet<Collider>(sets["leftcenter"]);
        sets["left"].ExceptWith(sets["rightcenter"]);

        sets["right"] = new HashSet<Collider>(sets["rightcenter"]);
        sets["right"].ExceptWith(sets["leftcenter"]);

        sets["front"] = new HashSet<Collider>(sets["frontmiddle"]);
        sets["front"].ExceptWith(sets["backmiddle"]);

        sets["back"] = new HashSet<Collider>(sets["backmiddle"]);
        sets["back"].ExceptWith(sets["frontmiddle"]);

        sets["frontLeft"] = new HashSet<Collider>(sets["front"]);
        sets["frontLeft"].ExceptWith(sets["left"]);

        sets["frontCenter"] = new HashSet<Collider>(sets["front"]);
        sets["frontCenter"].ExceptWith(sets["center"]);

        sets["frontRight"] = new HashSet<Collider>(sets["front"]);
        sets["frontRight"].ExceptWith(sets["right"]);

        sets["middleLeft"] = new HashSet<Collider>(sets["middle"]);
        sets["middleLeft"].ExceptWith(sets["left"]);

        sets["middleCenter"] = new HashSet<Collider>(sets["middle"]);
        sets["middleCenter"].ExceptWith(sets["center"]);

        sets["middleRight"] = new HashSet<Collider>(sets["middle"]);
        sets["middleRight"].ExceptWith(sets["right"]);

        sets["backLeft"] = new HashSet<Collider>(sets["back"]);
        sets["backLeft"].ExceptWith(sets["left"]);

        sets["backCenter"] = new HashSet<Collider>(sets["back"]);
        sets["backCenter"].ExceptWith(sets["center"]);

        sets["backRight"] = new HashSet<Collider>(sets["back"]);
        sets["backRight"].ExceptWith(sets["right"]);


        sets["nearFrontLeft"] = new HashSet<Collider>(sets["frontLeft"]);
        sets["nearFrontLeft"].ExceptWith(sets["near"]);

        sets["nearFrontCenter"] = new HashSet<Collider>(sets["frontCenter"]);
        sets["nearFrontCenter"].ExceptWith(sets["near"]);

        sets["nearFrontRight"] = new HashSet<Collider>(sets["frontRight"]);
        sets["nearFrontRight"].ExceptWith(sets["near"]);

        sets["nearMiddleLeft"] = new HashSet<Collider>(sets["middleLeft"]);
        sets["nearMiddleLeft"].ExceptWith(sets["near"]);

        sets["nearMiddleRight"] = new HashSet<Collider>(sets["middleRight"]);
        sets["nearMiddleRight"].ExceptWith(sets["near"]);

        sets["nearbackLeft"] = new HashSet<Collider>(sets["backLeft"]);
        sets["nearbackLeft"].ExceptWith(sets["near"]);

        sets["nearbackCenter"] = new HashSet<Collider>(sets["backCenter"]);
        sets["nearbackCenter"].ExceptWith(sets["near"]);

        sets["nearbackRight"] = new HashSet<Collider>(sets["backRight"]);
        sets["nearbackRight"].ExceptWith(sets["near"]);


        sets["farFrontLeft"] = new HashSet<Collider>(sets["frontLeft"]);
        sets["farFrontLeft"].ExceptWith(sets["far"]);

        sets["farFrontCenter"] = new HashSet<Collider>(sets["frontCenter"]);
        sets["farFrontCenter"].ExceptWith(sets["far"]);

        sets["farFrontRight"] = new HashSet<Collider>(sets["frontRight"]);
        sets["farFrontRight"].ExceptWith(sets["far"]);

        sets["farMiddleLeft"] = new HashSet<Collider>(sets["middleLeft"]);
        sets["farMiddleLeft"].ExceptWith(sets["far"]);

        sets["farMiddleRight"] = new HashSet<Collider>(sets["middleRight"]);
        sets["farMiddleRight"].ExceptWith(sets["far"]);

        sets["farbackLeft"] = new HashSet<Collider>(sets["backLeft"]);
        sets["farbackLeft"].ExceptWith(sets["far"]);

        sets["farbackCenter"] = new HashSet<Collider>(sets["backCenter"]);
        sets["farbackCenter"].ExceptWith(sets["far"]);

        sets["farbackRight"] = new HashSet<Collider>(sets["backRight"]);
        sets["farbackRight"].ExceptWith(sets["far"]);


        /*if (Random.Range(0f, 1f) < (1f/1000f))
        {
            foreach (KeyValuePair<string, HashSet<Collider>> kvp in sets)
            {
                string s = " ";
                foreach (var c in kvp.Value)
                {
                    s += c.gameObject.name + " ";
                }
                Debug.Log(transform.gameObject.name + " Set " + kvp.Key + ": " + kvp.Value.Count + s);
            }
        }*/
        return sets;
    }
}
