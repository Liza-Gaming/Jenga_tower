using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class TowerBoundary : MonoBehaviour
{
    public CameraMover cameraMover;  // Assign your CameraMover here.
    public float hoverOffset = 2.0f;   // How far above the tower top the block should hover.
    public Transform hoverTarget;    // The fixed hover target position (X, Z) above the tower.

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("JengaBlock"))
        {
            Block blockScript = other.GetComponent<Block>();
            // Only set hovering if the block hasn't been dropped.
            if (blockScript != null && !blockScript.isDropped)
            {
                // If a block is already hovering, reload the scene.
                if (BlockDragManager.isBlockHovering)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    return;
                }

                Debug.Log("Block exited the tower boundary.");

                if (cameraMover != null)
                    cameraMover.MoveCameraToTowerTop();

                float maxHeight = float.MinValue;
                GameObject[] blocks = GameObject.FindGameObjectsWithTag("JengaBlock");
                foreach (GameObject b in blocks)
                {
                    float blockTop = b.transform.position.y + (b.transform.localScale.y / 2);
                    if (blockTop > maxHeight)
                        maxHeight = blockTop;
                }
                float newHoverY = maxHeight + hoverOffset;

                Vector3 newPosition = other.transform.position;
                if (hoverTarget != null)
                {
                    newPosition.x = hoverTarget.position.x;
                    newPosition.z = hoverTarget.position.z;
                }
                newPosition.y = newHoverY;

                // Compute the target rotation based on dropped floor count.
                Vector3 targetEuler = new Vector3(-90, 0, 0);
                BlockDragManager dragManager = FindObjectOfType<BlockDragManager>();
                if (dragManager != null)
                {
                    int floorNumber = dragManager.droppedBlockCount / 3;
                    if (floorNumber % 2 != 0)
                        targetEuler = new Vector3(-90, 0, 90);
                }

                // Use RPCs to update the block on all clients.
                PhotonView pv = other.GetComponent<PhotonView>();
                pv.RPC("RPC_UpdateTransform", RpcTarget.All, newPosition, targetEuler);
                pv.RPC("RPC_SetHoveringState", RpcTarget.All, true, newHoverY);

                // Set the global hovering flag.
                BlockDragManager.isBlockHovering = true;
            }
        }
    }
}
