using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnnouncerVoice : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] fumbleClips;
    [SerializeField]
    private AudioClip[] catchClips;
    [SerializeField]
    private AudioClip[] interceptionClips;
    [SerializeField]
    private AudioClip[] noScoreClips;
    [SerializeField]
    private AudioClip[] winClips;
    [SerializeField]
    private AudioClip[] randomClips;
    [SerializeField]
    private AudioSource audioSource;


    private float lastVO = 0f;
    private float holdoff = 15f;
    private float holdoffBase = 8f;
    private float holdoffRandomness = 12f;
    private float normalChance = 0.2f;
    private float randomChance = 0.005f;
    private float noScoreChance = 0.0005f;

    public void TryRandomVO()
    {
        if(lastVO + holdoff < Time.time)
        {
            if(Random.Range(0f, 1f) < randomChance)
            {
                PlayVO(randomClips);
            }
        }
    }

    public void TryFumbleVO()
    {
        TryVO(fumbleClips);
    }

    public void TryCatchVO()
    {
        TryVO(catchClips);
    }
    public void TryInterceptionVO()
    {
        TryVO(interceptionClips);
    }
    public void TryWinVO()
    {
        TryVO(winClips);
    }
    public void TryNoScoreVO()
    {
        if (lastVO + holdoff < Time.time)
        {
            if (Random.Range(0f, 1f) < noScoreChance)
            {
                PlayVO(noScoreClips);
            }
        }
    }

    private void TryVO(AudioClip[] clips)
    {
        if (lastVO + holdoff < Time.time)
        {
            if (Random.Range(0f, 1f) < normalChance)
            {
                PlayVO(clips);
            }
        }
    }

    private void PlayVO(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        lastVO = Time.time;
        holdoff = Random.Range(0f, holdoffRandomness) + holdoffBase;
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
