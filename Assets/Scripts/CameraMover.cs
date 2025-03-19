using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    public Transform target; // The object the camera orbits.
    [HideInInspector]
    public Vector3 originalTargetPosition; // To restore later.
    private float currentTargetY; // The current target height.

    public float distance = 10.0f;
    public float minDistance = 5.0f;
    public float maxDistance = 15.0f;
    public float moveSpeed = 3.0f;
    public float zoomSpeed = 5.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    // Duration for smooth transitions.
    public float smoothTransitionTime = 1f;

    void Start()
    {
        originalTargetPosition = target.position;
        currentTargetY = originalTargetPosition.y;

        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
        }
        // For mouse input (Editor/testing)
        else if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (target)
        {
            // Check for mobile touch input.
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    // Rotate camera based on touch movement.
                    x += touch.deltaPosition.x * moveSpeed * 0.1f;
                    y -= touch.deltaPosition.y * moveSpeed * 0.1f;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Handle pinch zoom.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 prevTouchZeroPos = touchZero.position - touchZero.deltaPosition;
                Vector2 prevTouchOnePos = touchOne.position - touchOne.deltaPosition;
                float prevTouchDeltaMag = (prevTouchZeroPos - prevTouchOnePos).magnitude;
                float currentTouchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = prevTouchDeltaMag - currentTouchDeltaMag;

                distance += deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
            else
            {
                // Fallback PC input (useful for testing in Editor)
                x += Input.GetAxis("Horizontal") * moveSpeed;
                y -= Input.GetAxis("Vertical") * moveSpeed;

                distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// Smoothly moves the camera's target upward so that the tower top is visible.
    /// </summary>
    public void MoveCameraToTowerTop(float offset = 2.0f)
    {
        // Find the highest block.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("JengaBlock");
        float maxHeight = float.MinValue;
        foreach (GameObject block in blocks)
        {
            float blockTop = block.transform.position.y + (block.transform.localScale.y / 2);
            if (blockTop > maxHeight)
                maxHeight = blockTop;
        }

        // Compute the new target position above the highest block.
        Vector3 newTarget = target.position;
        newTarget.y = maxHeight + offset;
        // Start a smooth transition.
        StartCoroutine(SmoothMoveTarget(newTarget));
        currentTargetY = newTarget.y;
        Debug.Log("Camera target moving to: " + newTarget);
    }

    /// <summary>
    /// Smoothly increases the camera's target height (for a new floor).
    /// </summary>
    public void IncreaseTargetHeight(float additionalHeight)
    {
        // Add the additional height to the stored original target position.
        Vector3 newTarget = originalTargetPosition;
        newTarget.y += additionalHeight/2;

        // Update both current and original target positions.
        currentTargetY = newTarget.y;
        originalTargetPosition = newTarget;

        // Smoothly move the target to the new target position.
        StartCoroutine(SmoothMoveTarget(newTarget));

        Debug.Log("Camera target increased to: " + newTarget.y);
    }


    /// <summary>
    /// Smoothly restores the camera's target to its original position.
    /// </summary>
    public void RestoreCameraTarget()
    {
        StartCoroutine(SmoothMoveTarget(originalTargetPosition));
    }

    private IEnumerator SmoothMoveTarget(Vector3 endTarget)
    {
        Vector3 startPos = target.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / smoothTransitionTime;
            target.position = Vector3.Lerp(startPos, endTarget, t);
            yield return null;
        }
        target.position = endTarget;
        Debug.Log("Camera target reached: " + endTarget);
    }
}
