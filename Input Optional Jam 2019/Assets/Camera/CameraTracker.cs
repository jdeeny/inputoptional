using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public float maxHeight      = 30f;
    public float maxX     = 50f;
    public float maxZ     = 50f;
    public float followDistance = 3.5f;
    public Vector3 followOffset = new Vector3(0f,25f,-2f);

    public float cameraFollowSpeed = 2.5f; 

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

        Vector3 targetPos  = subject.transform.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos,
            cameraFollowSpeed / Vector3.Distance(transform.position, targetPos));

        if (transform.localPosition.y > maxHeight) transform.localPosition = new Vector3(
            transform.localPosition.x, maxHeight, transform.localPosition.z
        );
        
        if (transform.localPosition.x > maxX) transform.localPosition = new Vector3(
            maxX, transform.localPosition.y, transform.localPosition.z
        );

        if (transform.localPosition.x < (maxX * -1)) transform.localPosition = new Vector3(
            maxX * -1, transform.localPosition.y, transform.localPosition.z
        );

        if (transform.localPosition.z > maxZ) transform.localPosition = new Vector3(
            transform.localPosition.x, transform.localPosition.y, maxZ
        );

        if (transform.localPosition.z < (maxZ * -1)) transform.localPosition = new Vector3(
            transform.localPosition.x, transform.localPosition.y, (maxZ * -1)
        );
    }
}
