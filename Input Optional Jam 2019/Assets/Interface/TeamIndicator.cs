using UnityEngine;
using TMPro; 

public class TeamIndicator : MonoBehaviour
{
    public void UpdateName(string _name)
    {
        Transform teamName = transform.Find("TeamName"); 
        if (teamName != null)
        {
            TextMeshProUGUI text = teamName.GetComponent<TextMeshProUGUI>();
            if (text != null) text.text = _name.ToUpper(); 
        }
    }

    public void UpdateScore(int _score)
    {
        Transform score = transform.Find("Score");
        if (score != null)
        {
            TextMeshProUGUI text = score.GetComponent<TextMeshProUGUI>();
            if (text != null) text.text = _score.ToString();
        }
    }
}
