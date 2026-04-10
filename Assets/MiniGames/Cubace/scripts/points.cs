using UnityEngine;
using TMPro;

public class points : MonoBehaviour
{
    int point = 0;
    public TextMeshProUGUI pointstext;
    public void AddPoint()
    {
        point += 50;
        pointstext.text = point.ToString();
    }
    public void SubPoint()
    {
        point -= 10;
        pointstext.text = point.ToString();

    }
}
