using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject spotPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(spotPrefab, Vector3.zero, Quaternion.identity);
        Instantiate(playerPrefab, new Vector3(5f, 5f, 0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
