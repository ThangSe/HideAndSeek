using UnityEngine;

public static class Factory
{
    public static IState CreateEnemy(GameObject gameObject, Transform target, SOEnemyRefs enemyRefsSO, Vector3 pos)
    {
        return new NewEnemy(gameObject, target, enemyRefsSO, pos);
    }
    public static IWall CreateWall(GameObject gameObject, Vector3 pos, WallRefsSO wallRefsSO, bool isFinish, bool isFake)
    {
        return new NewWall(gameObject, pos, wallRefsSO, isFinish, isFake);
    }

    public static IState CreateSpawnWarning(GameObject gameObject, Vector3 pos)
    {
        return new NewEffectWarning(gameObject, pos);
    }

    public static IItem CreateItem(GameObject gameObject, Vector3 pos, SOItemRefs itemRefsSO)
    {
        return new NewItem(gameObject, pos, itemRefsSO);
    }

    public static IPopup CreatePopup(GameObject newObject)
    {
        return new NewPopup(newObject);
    }
}
