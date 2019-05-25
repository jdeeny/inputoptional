using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] footstepClips;
    [SerializeField]
    private AudioClip[] crashClips;

    private AudioSource audioSource;
    private AudioSource crashSource;
    private void Awake()
    {
        var sources = GetComponents<AudioSource>();
        audioSource = sources[0];
        crashSource = sources[1];
    }

    private void LeftFootstep()
    {
        AudioClip clip = GetRandomFootstepClip();
        audioSource.PlayOneShot(clip);
    }

    private void RightFootstep()
    {
        LeftFootstep();
    }

    private AudioClip GetRandomFootstepClip()
    {
        return footstepClips[Random.Range(0, footstepClips.Length)];
    }

    private AudioClip GetRandomCrashClip()
    {
        return crashClips[Random.Range(0, crashClips.Length)];
    }

    public void PlayCrash()
    {
        crashSource.PlayOneShot(GetRandomCrashClip());
    }
}
