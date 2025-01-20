using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs; // Array of NPC prefabs
    public int maxNPCs = 20; // Maximum NPCs to spawn
    public float mapRadius = 50f; // Radius of spawnable area

    void Start()
    {
        SpawnNPCs();
    }

    private void SpawnNPCs()
    {
        int spawnedCount = 0;

        while (spawnedCount < maxNPCs)
        {
            Vector3 randomPoint = new Vector3(
                transform.position.x + Random.Range(-mapRadius / 2f, mapRadius / 2f),
                transform.position.y,
                transform.position.z + Random.Range(-mapRadius / 2f, mapRadius / 2f)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                int randomIndex = Random.Range(0, npcPrefabs.Length);
                Instantiate(npcPrefabs[randomIndex], hit.position, Quaternion.identity);
                spawnedCount++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(mapRadius, 0, mapRadius));
    }
}