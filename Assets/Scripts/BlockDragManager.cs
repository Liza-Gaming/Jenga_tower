using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class BlockDragManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject uiPanel;       // Drag your control panel here
    public Button leftButton;
    public Button rightButton;
    public Button pushButton;

    [Header("Movement Settings")]
    public float moveDistance = 0.2f;  // For non-hovering movement
    public Camera mainCamera;        // Assign your main camera

    [Header("Discrete Hover Slots (for hovering blocks)")]
    public Transform hoverTarget;    // Center position for hovering block (assign in Inspector)
    public float slotDistance = 1f;    // How far left/right the slots are from the center

    private int hoverSlotIndex = 0;    // Allowed values: -1 (left), 0 (center), 1 (right)
    [HideInInspector]
    public int droppedBlockCount = 0;  // Count of dropped blocks

    public Transform selectedBlock;
    public static bool isBlockHovering = false;

    [SerializeField] private Text pushDropButton;

    void Awake()
    {
        isBlockHovering = false;
    }

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
        leftButton.onClick.AddListener(MoveLeft);
        rightButton.onClick.AddListener(MoveRight);
        pushButton.onClick.AddListener(PushOrDropBlock);
    }

    void Update()
    {
        if (selectedBlock != null)
        {
            Block blockScript = selectedBlock.GetComponent<Block>();
            if (blockScript != null)
            {
                pushDropButton.text = blockScript.isHovering ? "Drop" : "Push";
            }
        }
    }

    public void SetSelectedBlock(Transform block)
    {
        if (selectedBlock != null && selectedBlock != block)
        {
            Block prevBlock = selectedBlock.GetComponent<Block>();
            if (prevBlock != null)
                prevBlock.SetSelected(false);
        }
        selectedBlock = block;
        Block currentBlock = block.GetComponent<Block>();
        if (currentBlock != null)
        {
            currentBlock.SetSelected(true);
            hoverSlotIndex = 0;
        }
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
    }

    // Helper method to calculate the new position for a hovering block.
    Vector3 CalculateHoverSlotPosition()
    {
        int floorNumber = droppedBlockCount / 3;
        Vector3 centerPos = hoverTarget.position;
        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 newPos = centerPos;

        if (hoverSlotIndex == -1)
            newPos = centerPos - camRight * slotDistance;
        else if (hoverSlotIndex == 0)
            newPos = centerPos;
        else if (hoverSlotIndex == 1)
            newPos = centerPos + camRight * slotDistance;

        // Preserve the current Y (hover height)
        newPos.y = selectedBlock.position.y;
        return newPos;
    }

    void MoveLeft()
    {
        if (selectedBlock != null && mainCamera != null)
        {
            Block blockScript = selectedBlock.GetComponent<Block>();
            Vector3 newPos = selectedBlock.position;

            if (blockScript != null && blockScript.isHovering)
            {
                hoverSlotIndex = Mathf.Clamp(hoverSlotIndex - 1, -1, 1);
                newPos = CalculateHoverSlotPosition();
            }
            else
            {
                Vector3 leftDir = -mainCamera.transform.right;
                leftDir.y = 0;
                leftDir.Normalize();
                newPos += leftDir * moveDistance;
            }

            // Call RPC to update block position on all clients.
            selectedBlock.GetComponent<PhotonView>().RPC("RPC_MoveBlock", RpcTarget.All, newPos);
        }
    }

    void MoveRight()
    {
        if (selectedBlock != null && mainCamera != null)
        {
            Block blockScript = selectedBlock.GetComponent<Block>();
            Vector3 newPos = selectedBlock.position;

            if (blockScript != null && blockScript.isHovering)
            {
                hoverSlotIndex = Mathf.Clamp(hoverSlotIndex + 1, -1, 1);
                newPos = CalculateHoverSlotPosition();
            }
            else
            {
                Vector3 rightDir = mainCamera.transform.right;
                rightDir.y = 0;
                rightDir.Normalize();
                newPos += rightDir * moveDistance;
            }

            selectedBlock.GetComponent<PhotonView>().RPC("RPC_MoveBlock", RpcTarget.All, newPos);
        }
    }

    void PushOrDropBlock()
    {
        if (selectedBlock != null && mainCamera != null)
        {
            Block blockScript = selectedBlock.GetComponent<Block>();

            if (blockScript != null && blockScript.isHovering)
            {
                // Calculate the drop height based on the highest block.
                float maxHeight = float.MinValue;
                GameObject[] blocks = GameObject.FindGameObjectsWithTag("JengaBlock");
                foreach (GameObject b in blocks)
                {
                    if (b.transform == selectedBlock)
                        continue;
                    float blockTop = b.transform.position.y + (b.transform.localScale.y * 0.5f);
                    if (blockTop > maxHeight)
                        maxHeight = blockTop;
                }
                float blockHalfHeight = selectedBlock.localScale.y * 0.5f;
                float dropY = maxHeight + blockHalfHeight;
                Vector3 pos = selectedBlock.position;
                pos.y = dropY;

                // Choose a rotation based on the floor number.
                int floorNumber = droppedBlockCount / 3;
                Vector3 targetEuler = new Vector3(-90, 0, 0);
                if (floorNumber % 2 != 0)
                    targetEuler = new Vector3(-90, 0, 90);

                // Call RPC to drop the block.
                selectedBlock.GetComponent<PhotonView>().RPC("RPC_DropBlock", RpcTarget.All, pos, targetEuler);

                droppedBlockCount++;
                isBlockHovering = false;

                // Update camera target if needed.
                CameraMover cam = FindObjectOfType<CameraMover>();
                if (cam != null)
                {
                    cam.RestoreCameraTarget();
                    if (droppedBlockCount % 3 == 0)
                        cam.IncreaseTargetHeight(selectedBlock.localScale.y);
                }
            }
            else
            {
                // For non-hovering blocks, push them forward.
                Vector3 forwardDir = mainCamera.transform.forward;
                forwardDir.y = 0;
                forwardDir.Normalize();
                Vector3 newPos = selectedBlock.position + forwardDir * moveDistance;
                selectedBlock.GetComponent<PhotonView>().RPC("RPC_MoveBlock", RpcTarget.All, newPos);
            }
        }
    }

    // Optional: A method to snap the block to the correct slot.
    void SnapBlockToSlot()
    {
        if (selectedBlock == null) return;
        Vector3 newPos = CalculateHoverSlotPosition();
        selectedBlock.GetComponent<PhotonView>().RPC("RPC_MoveBlock", RpcTarget.All, newPos);
    }
}
