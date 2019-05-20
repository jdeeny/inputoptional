using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    public float pickupChance;
    public int ownerTeam = 0;
    public GameObject ownerPlayer = null;

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
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        transform.parent = ownerPlayer.transform;
    }

    public void Detach() {
        SetOwner(0, null);
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
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
}
