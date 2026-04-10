using UnityEngine;
using static UnityEngine.Object;

public static class SpaceObjectCreator
{
    public static SpaceObject CreateSpaceObject(GameObject obj, Transform parent)
    {
        GameObject instance = Instantiate(obj, parent);

        return AddToPhysicsManager(instance);
    }

    public static SpaceObject CreateSpaceObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        GameObject instance = Instantiate(obj, position, rotation);

        return AddToPhysicsManager(instance);
    }

    public static SpaceObject CreateSpaceObject(GameObject obj, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject instance = Instantiate(obj, position, rotation, parent);

        return AddToPhysicsManager(instance);
    }

    public static SpaceObject CreateSpaceObject(GameObject obj)
    {
        GameObject instance = Instantiate(obj);

        return AddToPhysicsManager(instance);
    }

    static SpaceObject AddToPhysicsManager(GameObject obj)
    {
        SpaceObject spaceObj = obj.GetComponent<SpaceObject>();
        PhysicsManager.Instance.objects.Add(spaceObj);
        return spaceObj;
    }
}
