using System.Collections;
using System.Collections.Generic;
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
        audioSource.PlayOneShot(fireSound);
    }
    public void PlaySound1()
    {
        Debug.Log("PlaySound1");
        audioSource.PlayOneShot(customSound1);
    }

    public void PlaySound2()
    {
        audioSource.PlayOneShot(customSound2);
    }

    public void PlaySound3()
    {
        audioSource.PlayOneShot(customSound3);
    }

    public void PlaySound4()
    {
        audioSource.PlayOneShot(customSound4);
    }

    public void PlaySound5()
    {
        audioSource.PlayOneShot(customSound5);
    }

    public void PlaySound6()
    {
        audioSource.PlayOneShot(customSound6);
    }

    public void PlaySound7()
    {
        audioSource.PlayOneShot(customSound7);
    }
}
