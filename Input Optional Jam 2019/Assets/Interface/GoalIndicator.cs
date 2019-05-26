using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GoalIndicator : MonoBehaviour
{
    public AnimationCurve showCurve;
    public float showLength = 6f;
    private float showTime;
    private bool active;
    private Image background;
    private TextMeshProUGUI text; 

    private void Awake()
    {
        showTime = 0f; 
        active   = false;

        background = this.transform.Find("Background").GetComponent<Image>();
        text = this.transform.Find("Text").GetComponent<TextMeshProUGUI>(); 
    }

    public void ShowScore(string scoringPlayer, string scoringTeam, int score)
    {
        active   = true;
        showTime = 0f; 
    }

    //Cheap way to track the timer
    public void Update()
    {
        if (active)
        {
            showTime += Time.deltaTime;
            background.color = new Color(background.color.r, 
                background.color.g, 
                background.color.b, 
                showCurve.Evaluate(showTime/showLength));
            text.color = new Color(text.color.r,
                text.color.g,
                text.color.b,
                showCurve.Evaluate(showTime/showLength));
            if (showTime > showLength) active = false; 
        }
    }
}
