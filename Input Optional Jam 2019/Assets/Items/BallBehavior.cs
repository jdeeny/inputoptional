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
        throwDisableTimeout -= Time.deltaTime;
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
        transform.parent = ownerPlayer.transform;
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

        rb.AddForce(new Vector3(Random.Range(-500f, 500f), Random.Range(30f, 300f), Random.Range(-500f, 500f)));
    }

    public void Reset() {
        Detach();
        transform.position = new Vector3(0f, 5f, 0f);
        rb.isKinematic = true;
    }

    public void ThrowTo(Vector3 location)
    {
        //Debug.Log("Throwing Ball");
        throwDisableTimeout = 0.1f;
        Detach();
        rb.AddForce(new Vector3(Random.Range(-200f, 200f), Random.Range(2f, 20f), Random.Range(-200f, 200f)));
    }

}
