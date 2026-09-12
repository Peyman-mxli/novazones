using UnityEngine;

public class NovaAngelRespawn : MonoBehaviour
{
    [Header("Angel Info")]
    public string angelName = "NovaAngel";

    [Header("Respawn Point")]
    public Transform spawnPoint;

    public Transform GetSpawnPoint()
    {
        if (spawnPoint != null)
            return spawnPoint;

        return transform;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 drawPos = transform.position;

        if (spawnPoint != null)
        {
            drawPos = spawnPoint.position;
        }

        Gizmos.DrawWireSphere(drawPos, 1f);
    }
}