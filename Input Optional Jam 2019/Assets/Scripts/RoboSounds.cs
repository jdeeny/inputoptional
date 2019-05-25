using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] footstepClips;
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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
}
