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
    public Vector3 closeOffset = new Vector3(0f, 3f, 10f);
    public Vector3 closeRotation = new Vector3(30f, 0f, 0f); 

    public float cameraFollowSpeed = 2.5f;
    public float cameraLookSpeed = 3f;

    public Transform panPoint; 
    
    private Vector3 baseRotation;
    private float timeSinceModeSwitch;
    public float modeSwitchLength = 2f; 

    public enum CameraMode
    {
        FollowBall,
        CloseUp,
        Pan
    };
    private CameraMode mode; 

    public void SetMode(CameraMode _mode)
    {
        mode = _mode;
        timeSinceModeSwitch = 0f; 
    }

    private void Awake()
    {
        timeSinceModeSwitch = 0f;
        baseRotation = transform.localEulerAngles;  
        mode = CameraMode.FollowBall; 
    }

    private void Update()
    {
        if (timeSinceModeSwitch < modeSwitchLength) timeSinceModeSwitch += Time.deltaTime;

        switch (mode)
        {
            case CameraMode.FollowBall:
                {
                    transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, baseRotation, timeSinceModeSwitch/modeSwitchLength); 
                    PanToPoint(GameManager.Instance.ball, followOffset); 
                    break; 
                }

            case CameraMode.CloseUp:
                {
                    transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, closeRotation, timeSinceModeSwitch/modeSwitchLength);
                    PanToPoint(GameManager.Instance.ball, closeOffset);
                    break;
                }

            case CameraMode.Pan:
                {
                    transform.position = panPoint.position;
                    LookAtPoint(GameManager.Instance.ball); 
                    break;
                }
        }
    }

    private void LookAtPoint(GameObject subject)
    {
        transform.LookAt(subject.transform); 
    }

    private void PanToPoint(GameObject subject, Vector3 offset)
    {
        //Ball might not exist
        if (subject == null) return;

        Vector3 targetPos  = subject.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos,
            cameraFollowSpeed);

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
