using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource sounds;

    public AudioClip blockCollision;
    public AudioClip fallingOnGround;
    public void PlaySFX(AudioClip clip)
    {
        sounds.PlayOneShot(clip);
    }
    public void PlaySFXLower(AudioClip clip)
    {
        sounds.PlayOneShot(clip,0.5f);
    }

}