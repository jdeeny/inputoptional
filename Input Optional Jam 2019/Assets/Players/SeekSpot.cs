using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekSpot : MonoBehaviour
{

    public float thrust;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(GameManager.Instance.spot.transform.position);
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.StartsWith("Spot"))
        {
            Destroy(gameObject);
        }
    }
}
