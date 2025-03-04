using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstepSound : MonoBehaviour
{
    //declaring the audio source
    public AudioSource audioFootstepPlayer;

    // Start is called before the first frame update
    void Start()
    {
        //get the components from the audio source from the player
     audioFootstepPlayer = GetComponent<AudioSource>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void footstepSound()
    {
        //play the footstep sounds
        audioFootstepPlayer.Play();
    }
}
