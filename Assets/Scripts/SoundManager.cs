using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip coinCollect;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip rollForward;
    [SerializeField] private AudioClip laneChange;
    [SerializeField] private AudioClip footstep1;
    [SerializeField] private AudioClip footstep2;
    [SerializeField] private AudioClip castMagic;
    [SerializeField] private AudioClip death;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float backgroundMusicVolume = 0.2f;

    private AudioSource effectSource;
    private AudioSource musicSource;

    private void Awake()
    {
        // Singleton pattern check
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize the AudioSources
        effectSource = GetComponent<AudioSource>();

        // Create a new AudioSource for background music.
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = backgroundMusicVolume;
    }

    private void Start()
    {
        // Start playing the background music.
        if (backgroundMusic != null)
        {
            musicSource.Play();
        }
    }

    private void Update()
    {
        // Update the volume continuously in case backgroundMusicVolume changes
        musicSource.volume = backgroundMusicVolume;
    }

    // Sound Effect Methods
    public void soundCoinCollect()
    {
        effectSource.PlayOneShot(coinCollect);
    }
    
    public void soundJump()
    {
        effectSource.PlayOneShot(jump);
    }
    
    public void soundLand()
    {
        effectSource.PlayOneShot(land);
    }
    
    public void soundRollForward()
    {
        Debug.Log("Rolling forward sound");
        effectSource.PlayOneShot(rollForward);
    }
    
    public void soundLaneChange()
    {
        effectSource.PlayOneShot(laneChange);
    }
    
    public void soundFootstep1()
    {
        effectSource.PlayOneShot(footstep1);
    }
    
    public void soundFootstep2()
    {
        effectSource.PlayOneShot(footstep2);
    }
    
    public void soundCastMagic()
    {
        effectSource.PlayOneShot(castMagic);
    }
    
    public void soundBlast()
    {
        // TODO: Implement blast sound
    }

    public void soundDeath()
    {
        effectSource.PlayOneShot(death);
    }
    
    // Add this method to your existing SoundManager class
    public void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
    {
       //todo effectSource.PlayOneShot(clip, volumeScale);
    }

}
