using UnityEngine;

public class CubaceCameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public static CubaceCameraFollow Instance;
    private bool isFrozen = false;
    private Vector3 frozenPosition;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isFrozen)
        {
            transform.position = frozenPosition;
        }
        else
        {
            transform.position = player.position + offset;
        }
    }

    public void FreezeCamera()
    {
        frozenPosition = transform.position;
        isFrozen = true;
    }

    public void UnfreezeCamera()
    {
        isFrozen = false;
    }
}
