using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverTrigger : MonoBehaviour
{

    private void OnTriggerExit(Collider other)
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
            }
        }
    }
}

