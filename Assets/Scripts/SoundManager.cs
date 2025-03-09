using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance
    {
        get;
        private set;
    }

    [SerializeField] private AudioClip coinCollect;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip rollForward;
    [SerializeField] private AudioClip laneChange;
    [SerializeField] private AudioClip footstep1;
    [SerializeField] private AudioClip footstep2;
    [SerializeField] private AudioClip castMagic;
    [SerializeField] private AudioClip death;
    
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void soundCoinCollect()
    {
        audioSource.PlayOneShot(coinCollect);
    }
    
    public void soundJump()
    {
        audioSource.PlayOneShot(jump);
    }
    
    public void soundLand()
    {
       audioSource.PlayOneShot(land);
    }
    
    public void soundRollForward()
    {
        Debug.Log("Rolling forward sound");
        audioSource.PlayOneShot(rollForward);
    }
    
    public void soundLaneChange()
    {
        audioSource.PlayOneShot(laneChange);
    }
    
    public void soundFootstep1()
    {
        audioSource.PlayOneShot(footstep1);
    }
    
    public void soundFootstep2()
    {
        audioSource.PlayOneShot(footstep2);
    }
    
    public void soundCastMagic()
    {
       audioSource.PlayOneShot(castMagic);
    }
    
    public void soundBlast()
    {
        //todo PlaySound(SoundType.BLAST);
    }

    public void soundDeath()
    {
        audioSource.PlayOneShot(death);
    }
}
