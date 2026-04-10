using UnityEngine;

public class gate : MonoBehaviour
{
    public points points;
    void OnTriggerEnter(Collider ColliderInfo)
    {
        if (ColliderInfo.GetComponent<Collider>().tag == "gate")
        {
            FindFirstObjectByType<points>().AddPoint();
            Debug.Log("points added");
        }
    }
    
}
