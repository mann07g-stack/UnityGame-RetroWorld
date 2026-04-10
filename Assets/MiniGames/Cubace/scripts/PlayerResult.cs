[System.Serializable]
public class PlayerResult
{
    public string name;     // Player's name
    public string appNo;    // Application number
    public int collisions;  // Collision count
    public float time;      // Completion time

    public PlayerResult(string name, string appNo, int collisions, float time)
    {
        this.name = name;
        this.appNo = appNo;
        this.collisions = collisions;
        this.time = time;
    }
}
