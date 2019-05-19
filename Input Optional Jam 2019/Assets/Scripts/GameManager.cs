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
        spot = Instantiate(spotPrefab, Vector3.zero, Quaternion.identity);
        players.Add(Instantiate(playerPrefab, new Vector3(5f, 5f, 0f), Quaternion.identity));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
