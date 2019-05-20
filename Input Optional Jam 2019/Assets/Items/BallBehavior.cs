using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallBehavior : MonoBehaviour
{
    public float pickupChance;
    public int ownerTeam = 0;
    public GameObject ownerPlayer = null;

    Rigidbody rb;
    Collider col;

    void Start() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update() {
        Debug.Log(GetFinalPosition());
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player" )
        {
            AttemptPickup(col.gameObject);
        }
    }

    void AttemptPickup(GameObject player)
    {
        //if(Random.Range(0f, 1f) < pickupChance)
        {
            SetOwner(player.GetComponent<PlayerAI>().GetTeam(), player);
            Debug.Log("Picked up" + ownerTeam);
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
            return (float)Math.Sqrt(2f * GetHeight() / 9.81f);
        }
        return 0f;
    }
    public Vector3 GetFinalPosition() {
        Vector3 velocity = GetVelocity();
        velocity.y = 0f;
        Vector3 distance = velocity * GetTimeToLand();
        Vector3 pos = transform.position;
        pos.y = 0f;
        return distance + pos;   
    }
}
