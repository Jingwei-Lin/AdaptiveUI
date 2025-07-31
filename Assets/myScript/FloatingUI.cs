using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public Transform targetCamera; // Assign XR Origin's Main Camera
    public float followSpeed = 3f;
    public Vector3 positionOffset = new Vector3(0, 0, 0.5f);
    public float horizontalOffset = 0f; // Added horizontal offset control

    public WalkDetector walkDetector; // Reference to WalkDetector script

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // Calculate target position with horizontal offset
        Vector3 targetPosition = targetCamera.position +
            targetCamera.forward * positionOffset.z +
            targetCamera.up * positionOffset.y +
            targetCamera.right * horizontalOffset; // Added horizontal offset

        // Face towards camera while maintaining upright rotation
        Quaternion targetRotation = Quaternion.LookRotation(
            targetCamera.forward,
            Vector3.up
        );

        Vector3 newPosition = transform.position;
        newPosition.x = targetPosition.x;
        newPosition.y = Mathf.Lerp(
            transform.position.y,
            targetPosition.y,
            followSpeed * Time.deltaTime
        );
        newPosition.z = targetPosition.z;
        transform.position = newPosition;
        
        
        // // Smooth movement
        // transform.position = Vector3.Lerp(
        //     transform.position,
        //     targetPosition,
        //     followSpeed * Time.deltaTime
        // );
        
        // Smoothly rotate towards the camera
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            followSpeed * Time.deltaTime
        );
    }
}