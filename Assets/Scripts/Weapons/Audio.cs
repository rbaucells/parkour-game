using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Audio : MonoBehaviour
{
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioClip customSound1;
    [SerializeField] AudioClip customSound2;
    [SerializeField] AudioClip customSound3;
    [SerializeField] AudioClip customSound4;
    [SerializeField] AudioClip customSound5;
    [SerializeField] AudioClip customSound6;
    [SerializeField] AudioClip customSound7;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FireSound()
    {
        // Play the fire sound
        audioSource.PlayOneShot(fireSound);
    }

    public void PlaySound1([Optional] float delay)
    {
        PlaySoundWithDelay(customSound1, delay);
    }

    public void PlaySound2([Optional] float delay)
    {
        PlaySoundWithDelay(customSound2, delay);
    }

    public void PlaySound3([Optional] float delay )
    {
        PlaySoundWithDelay(customSound3, delay);
    }

    public void PlaySound4([Optional] float delay)
    {
        PlaySoundWithDelay(customSound4, delay);
    }

    public void PlaySound5([Optional] float delay)
    {
        PlaySoundWithDelay(customSound5, delay);
    }

    public void PlaySound6([Optional]float delay)
    {
        PlaySoundWithDelay(customSound6, delay);
    }

    public void PlaySound7([Optional] float delay)
    {
        PlaySoundWithDelay(customSound7, delay);
    }

    private void PlaySoundWithDelay(AudioClip clip, float delay)
    {
        if (delay <= 0f )
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            StartCoroutine(PlaySoundAfterDelay(clip, delay));
        }
    }

    private IEnumerator PlaySoundAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }
}
