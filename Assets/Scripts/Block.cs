using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviourPunCallbacks
{
    private Renderer rend;
    private Material originalMaterial;
    public Material highlightMaterial;

    public bool isSelected = false;
    public bool isHovering = false;
    public bool isDropped = false;
    private float hoverY = 0f;
    public float hoverAdjustSpeed = 10f;

    // These variables hold the last received target state from RPCs.
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    // Smoothing factor for remote objects (tweak as needed).
    public float remoteSmoothing = 0.1f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    public void SetHovering(bool hovering, float targetHoverY = 0f)
    {
        if (isDropped && hovering)
            return;

        isHovering = hovering;
        if (hovering)
        {
            hoverY = targetHoverY;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            rend.material = originalMaterial;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            // Only the owner actively interpolates the hover Y value.
            if (isHovering)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, hoverY, Time.deltaTime * hoverAdjustSpeed);
                transform.position = pos;
                // Update target position so remote clients can smooth toward it.
                targetPosition = pos;
            }
        }
        else
        {
            // Remote clients smoothly update position and rotation using the last received target.
            transform.position = Vector3.Lerp(transform.position, targetPosition, remoteSmoothing);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, remoteSmoothing);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected && !isHovering)
            rend.material = highlightMaterial;
        else
            rend.material = originalMaterial;
    }

    void OnMouseEnter()
    {
        if (!isHovering && !isDropped && !isSelected)
            rend.material = highlightMaterial;
    }

    void OnMouseExit()
    {
        if (!isHovering && !isDropped && !isSelected)
            rend.material = originalMaterial;
    }

    void OnMouseDown()
    {
        if (BlockDragManager.isBlockHovering && !isSelected)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Request ownership so that this client can send RPCs.
        if (photonView != null)
            photonView.RequestOwnership();

        BlockDragManager dragManager = FindObjectOfType<BlockDragManager>();
        if (dragManager != null)
        {
            dragManager.SetSelectedBlock(transform);
        }
    }

    // --- RPC Methods for Discrete Moves ---

    [PunRPC]
    public void RPC_MoveBlock(Vector3 newPosition)
    {
        transform.position = newPosition;
        targetPosition = newPosition;
    }

    [PunRPC]
    public void RPC_SetRotation(Vector3 newEuler)
    {
        Quaternion rot = Quaternion.Euler(newEuler);
        transform.rotation = rot;
        targetRotation = rot;
    }

    [PunRPC]
    public void RPC_DropBlock(Vector3 newPosition, Vector3 newEuler)
    {
        transform.position = newPosition;
        Quaternion rot = Quaternion.Euler(newEuler);
        transform.rotation = rot;
        targetPosition = newPosition;
        targetRotation = rot;
        isDropped = true;
        SetHovering(false);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    [PunRPC]
    public void RPC_UpdateTransform(Vector3 pos, Vector3 euler)
    {
        targetPosition = pos;
        targetRotation = Quaternion.Euler(euler);
        // If the owner is updating, also snap immediately.
        if (photonView.IsMine)
        {
            transform.position = pos;
            transform.rotation = Quaternion.Euler(euler);
        }
    }

    [PunRPC]
    public void RPC_SetHoveringState(bool hovering, float newHoverY)
    {
        SetHovering(hovering, newHoverY);
    }
}
