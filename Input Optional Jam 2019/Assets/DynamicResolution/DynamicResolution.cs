using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicResolution : MonoBehaviour
{
    public Camera outputCamera;
    private RenderTexture texture;
    private RawImage image; 

    public float minimumResScale = 0.25f;
    public int lowFrameThreshold = 30;
    public int targetFramerate = 60;
    public int framesToParse = 600; 

    private int baseWidth;
    private int baseHeight;

    private int currentFrame;
    private float currentDelta;

    private void Start()
    {
        image = GetComponent<RawImage>(); 

        if (outputCamera == null || image == null)
        {
            Debug.LogError("Failed to get target camera for dynamic resolution.");
            GameObject.Destroy(this.gameObject);
            return; 
        }

        //Get the base resolutions
        baseWidth = Screen.width;
        baseHeight = Screen.height;

        SetResolution(1); 
    }

    private void Update()
    {
        ++currentFrame;
        currentDelta += Time.deltaTime;

        if (currentFrame >= framesToParse) CheckResolution(); 
    }

    private void CheckResolution()
    {
        float averageDelta = currentDelta / currentFrame;
        float framerate = 1 / averageDelta;

        Debug.Log("RESINFO [FRAMERATE]" + framerate + " [DELTA AVG]" + averageDelta + " [CURRFRAME]" + currentFrame + " [CURRDELTA]" + currentDelta); 
        if (framerate >= targetFramerate)
        {
            SetResolution(1);
        }
        else if (framerate < lowFrameThreshold) 
        {
            SetResolution(minimumResScale);
        }
        else
        {
            SetResolution(framerate / targetFramerate); 
        }
    }

    private void SetResolution(float scale)
    {
        Debug.Log("Resetting resolution to scale: " + scale); 

        if (scale < minimumResScale) scale = minimumResScale;

        int width  = Mathf.FloorToInt(baseWidth * scale);
        int height = Mathf.FloorToInt(baseHeight * scale);

        texture = new RenderTexture(width, height, 24);
        texture.filterMode = FilterMode.Point; 

        outputCamera.targetTexture = texture;
        image.texture = texture; 

        currentDelta = 0;
        currentFrame = 0; 
    }
}
