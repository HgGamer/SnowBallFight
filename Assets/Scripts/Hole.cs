using UnityEngine;

public class Hole : MonoBehaviour
{
    private float targetScale = 3.9f;
    private float growSpeed = 3.0f;
    private Vector3 initialRotation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        initialRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase size until target
        if (transform.localScale.x < targetScale)
        {
            float newScale = Mathf.Min(transform.localScale.x + growSpeed * Time.deltaTime, targetScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
        
        // Make hole match camera's Y rotation
        if (Camera.main != null)
        {
            // Simply copy camera's Y rotation while keeping original X and Z
            float cameraYRotation = Camera.main.transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(initialRotation.x, cameraYRotation+180, initialRotation.z);
        }
    }
}
