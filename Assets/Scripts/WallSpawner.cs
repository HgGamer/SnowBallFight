using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private int numberOfWalls = 8;
    [SerializeField] private float distanceFromCenter = 10f;
    
    public void SpawnWalls()
    {
        for (int i = 0; i < numberOfWalls; i++)
        {
            // Calculate the angle for each wall
            float angleInDegrees = i * (360f / numberOfWalls);
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            
            // Calculate the position based on the angle and distance
            float xPosition = distanceFromCenter * Mathf.Sin(angleInRadians);
            float zPosition = distanceFromCenter * Mathf.Cos(angleInRadians);
            Vector3 wallPosition = new Vector3(xPosition, 0, zPosition);
            
            // Instantiate the wall
            GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
            
            // Make the wall face the center
            wall.transform.LookAt(transform.position);
            // Rotate it 180 degrees to properly face inward
            wall.transform.Rotate(0, 180f, 0);
        }
    }
    
    private void Start()
    {
        SpawnWalls();
    }
}
