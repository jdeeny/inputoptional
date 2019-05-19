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
            Destroy(gameObject);
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
        players.Add(Instantiate(playerPrefab, new Vector3(Random.Range(-10.0f, 10.0f), 0.5f, Random.Range(-10.0f, 10.0f)), Quaternion.identity));

    }

    void OnCollisionEnter(Collision col)
    {
        print(col.gameObject.name);
        if (col.gameObject.name.StartsWith("Spot"))
        {
            Destroy(gameObject);
        }
    }
}
