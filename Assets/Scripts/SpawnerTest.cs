using UnityEngine;

public class SpawnerTest : MonoBehaviour
{ 
    public GameObject objectToSpawn; // The object to spawn
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(objectToSpawn, SpawnPosition, transform.rotation);
    }
    public Vector3 SpawnPosition;
    // Update is called once per frame
    void Update()
    {
        
    }
}
