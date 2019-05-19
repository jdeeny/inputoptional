using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : ScriptableObject
{
    public List<GameObject> players = new List<GameObject>();

    public void Init(GameObject playerPrefab, int players, Color c)
    {
        for (int i = 0; i < players; i++)
        {
            addPlayer(playerPrefab);
        }
    }

    public static Team CreateInstance(GameObject prefab, int players, Color color)
    {
        var team = ScriptableObject.CreateInstance<Team>();
        team.Init(prefab, players, color);
        return team;
    }

    void addPlayer(GameObject playerPrefab)
    {
        players.Add(Instantiate(playerPrefab, new Vector3(Random.Range(-50.0f, 50.0f), 0.5f, Random.Range(-50.0f, 50.0f)), Quaternion.identity));
    }
}
