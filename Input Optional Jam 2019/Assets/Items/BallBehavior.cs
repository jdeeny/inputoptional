using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class BallBehavior : MonoBehaviour
{
    public float pickupChance = 1f;
    public int ownerTeam = 0;
    public GameObject ownerPlayer = null;

    float throwDisableTimeout = 0f;

    Rigidbody rb;
    Collider col;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update() {
        if(transform.parent == null || transform.parent.gameObject.layer != 10) {
            if(transform.position.y <= -1f) {
                Debug.Log("Reset Ball Position " + transform.position);
                var newPos = transform.position;
                newPos.y = 0.5f;
                transform.position = newPos;
            }
        } else {
            transform.localPosition = new Vector3(.7f, 0f, .3f);
        }
        if (transform.position.y < -10f) Reset();
        throwDisableTimeout -= Time.deltaTime;
    }
    void FixedUpdate() {
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player" )
        {
            //Debug.Log(" Player Ball Collision");
            AttemptPickup(col.gameObject);
        }
    }

    void AttemptPickup(GameObject player)
    {
        Vector3 ball_vel = rb.velocity;
        Vector3 player_vel = player.GetComponent<Rigidbody>().velocity;
        Vector3 vel_diff = ball_vel - player_vel;
        float diff = (float) vel_diff.magnitude + 0.0001f;
        float adj_diff = (float) System.Math.Sqrt(diff / 4f);
        float chance = pickupChance * (1f / adj_diff);
        //Debug.Log("Vel diff: " + diff + " Adj: " + adj_diff +  " chance: " + chance);

        if (throwDisableTimeout <= 0f && Random.Range(0f, 1f) < chance)
        {
            SetOwner(player.GetComponent<PlayerAI>().GetTeam(), player);
            //Debug.Log("Picked up" + ownerTeam);
            AttachToPlayer(player);
        }
    }

    void AttachToPlayer(GameObject player) {
        col.enabled = false;
        rb.isKinematic = true;
        transform.parent = ownerPlayer.GetComponent<PlayerAI>().hand;
        transform.localPosition = new Vector3(.7f, 0f, .3f);
    }

    public void Detach() {
        SetOwner(0, null);
        col.enabled = true;
        rb.isKinematic = false;
        transform.parent = null;
    }

    public void SetOwner(int team, GameObject player)
    {
        ownerTeam = team;
        ownerPlayer = player;
    }

    public int GetOwnerTeam()
    {
        return ownerTeam;
    }

    public ref GameObject GetOwnerPlayer()
    {
        return ref ownerPlayer;
    }

    public float GetHeight() { 
        return transform.position.y - 0.5f;
        // FIXME: Should account for ball's size
    }
    public Vector3 GetVelocity() {
        return rb.velocity;
    }
    public float GetTimeToLand() {
        if(GetHeight() > 0f) {
            return (float)System.Math.Sqrt(2f * GetHeight() / 9.81f);
        }
        return 0f;
    }
    public Vector3 GetFinalPosition() {
        //FIXME: what about when it's going up? It needs to account for y velocity
        Vector3 velocity = GetVelocity();
        velocity.y = 0f;
        Vector3 distance = velocity * GetTimeToLand();
        Vector3 pos = transform.position;
        pos.y = 0f;
        return distance + pos;   
    }

    public void DoKickoff() {
        if(rb == null) {
            rb = GetComponent<Rigidbody>();
        }
        transform.position = new Vector3(0f, 5f, 0f);

        rb.isKinematic = false;

        rb.AddForce(new Vector3(Random.Range(-200f, 200f), Random.Range(50f, 200f), Random.Range(-200f, 200f)));
        rb.AddTorque(new Vector3(Random.Range(-200f, 200f), Random.Range(-200f, 200f), Random.Range(-200f, 200f)));
    }

    public void Reset() {
        Detach();
        transform.position = new Vector3(0f, 5f, 0f);
        rb.isKinematic = true;
    }

    public void ThrowTo(Vector3 location)
    {
        if(throwDisableTimeout > 0f)
        {
            return;
        }
        Debug.Log("Throwing Ball to: " + location);

        Vector3 dist = location - transform.position;
        dist.y = 0f;
        float distx = dist.magnitude;

        float airTime = Random.Range(2f, 4f);
        float vx = distx / airTime;
        float vy = airTime * 9.81f * 0.5f;

        Vector3 result = dist.normalized * vx;
        result.y = vy;

        Debug.Log("dist: " + dist + " dx: " + distx + " time: " + airTime + " " + result);

        throwDisableTimeout = 0.1f;
        Detach();
        rb.velocity = Vector3.zero;
        rb.AddForce(result, ForceMode.VelocityChange);
    }

}
