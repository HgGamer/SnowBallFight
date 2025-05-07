using UnityEngine;

public class CameraController : MonoBehaviour
{
    public  float lerpSpeed = 40f; // Adjust for smoothness
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0, 10.76f, -8.74f); // Default offset
    private float yaw = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target != null)
        {
            // Set initial position with offset
            transform.position = target.position + offset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.Local == null || !GameManager.IsConnected() || target == null)
        {
            return;
        }

        // Get the player's Y rotation
        float playerYaw = target.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, playerYaw, 0);
        Vector3 rotatedOffset = rotation * offset;
        Vector3 desiredPosition = target.position + rotatedOffset;
       
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * lerpSpeed);

        // Lerp rotation to look at the target
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * lerpSpeed);
    }
}
