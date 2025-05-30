using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    int points = 0;
    public void AddPoints(int points)
    {
        if(points > 0) this.points += points;
        Debug.Log(this.points);
    }
}
