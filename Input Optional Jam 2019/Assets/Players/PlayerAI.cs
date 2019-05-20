using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : MonoBehaviour
{

    public float thrust;
    int team = 0;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 0.5f)
        {
            transform.LookAt(GameManager.Instance.spot.transform.position);
            GetComponent<Rigidbody>().AddForce(transform.forward * thrust, ForceMode.Impulse);
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

    public void SetTeam(int team_num)
    {
        team = team_num;
    }

    public int GetTeam()
    {
        return team;
    }
}
