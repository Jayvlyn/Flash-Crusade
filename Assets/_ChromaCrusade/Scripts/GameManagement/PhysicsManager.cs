using UnityEngine;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour
{
    public Transform player;
    public bool usePlayerDist;

    public List<SpaceObject> objects = new();

    private void FixedUpdate()
    {
        foreach(SpaceObject obj in objects)
        {
            if(usePlayerDist && player != null)
            {
                float dist = Vector2.Distance(obj.transform.position, player.position);

                // temporary segments
                float step = dist < 20f ? 0.02f :
                             dist < 50f ? 0.04f :
                             0.08f;
                //-------------------

                obj.SetUpdateInterval(step);
            }
            obj.Tick(Time.fixedDeltaTime);
        }
    }
}
