using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public float followDistance = 3.5f;
    public Vector3 followOffset = new Vector3(0f,25f,-2f); 

    private enum CameraMode
    {
        FollowBall,
        Pan
    };
    private CameraMode mode; 

    private void Awake()
    {
        mode = CameraMode.FollowBall; 
    }

    private void Update()
    {
        switch (mode)
        {
            case CameraMode.FollowBall:
                {
                    PanToPoint(GameManager.Instance.ball); 
                    break; 
                }

            case CameraMode.Pan:
                {
                    break;
                }
        }
    }

    private void PanToPoint(GameObject subject)
    {
        //Ball might not exist
        if (subject == null) return;

        this.transform.localPosition = subject.transform.position + followOffset;
    }
}
