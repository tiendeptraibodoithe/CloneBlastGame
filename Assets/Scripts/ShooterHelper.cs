using UnityEngine;

public class ShooterHelper : MonoBehaviour
{
    public static Block FindNearestHittableBlock()
    {
        Block nearestBlock = null;
        float nearestDistance = float.MaxValue;

        Block[] allBlocks = FindObjectsOfType<Block>();

        foreach (Block block in allBlocks)
        {
            if (block.CanBeHit)
            {
                float distance = Vector3.Distance(Vector3.zero, block.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBlock = block;
                }
            }
        }

        return nearestBlock;
    }

    public static bool HasHittableBlocks()
    {
        Block[] allBlocks = FindObjectsOfType<Block>();

        foreach (Block block in allBlocks)
        {
            if (block.CanBeHit)
            {
                return true;
            }
        }

        return false;
    }
}
