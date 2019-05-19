using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; set; }

    public GameObject playerPrefab;
    public GameObject spotPrefab;

    public GameObject spot;
    public List<GameObject> players = new List<GameObject>();

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
    }

    // Update is called once per frame
    void Update()
    {
        if(Random.Range(0.0f, 1.0f) < newPlayerChance)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        players.Add(Instantiate(playerPrefab, new Vector3(Random.Range(-50.0f, 50.0f), 0.5f, Random.Range(-50.0f, 50.0f)), Quaternion.identity));

    }
}
