using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; set; }

    public GameObject playerPrefab;
    public GameObject spotPrefab;
    public int playersPerTeam;

    public GameObject spot;
    public List<Team> teams = new List<Team>();

    public float newPlayerChance;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.Log("Only one of this object is permitted");
            Destroy(gameObject);
            return;
        }
        spot = Instantiate(spotPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity);

        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, Color.blue));
        teams.Add(Team.CreateInstance(playerPrefab, playersPerTeam, Color.red));

    }

    // Update is called once per frame
    void Update()
    {
        if(Random.Range(0.0f, 1.0f) < newPlayerChance)
        {
            //SpawnPlayer();
        }
    }

}
