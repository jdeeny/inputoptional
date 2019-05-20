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
        if(Random.Range(0f, 1f) < pickupChance)
        {
            ownerTeam = player.GetComponent<PlayerAI>().GetTeam();
            ownerPlayer = player;
        }
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
