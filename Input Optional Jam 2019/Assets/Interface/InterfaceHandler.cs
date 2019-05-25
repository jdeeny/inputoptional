using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceHandler : MonoBehaviour
{

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.LogWarning(NameGenerator.GenerateCityName()); 
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.LogWarning(NameGenerator.GenerateRobotName());
        }
    }
}
