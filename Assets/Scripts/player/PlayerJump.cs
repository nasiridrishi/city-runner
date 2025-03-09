using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        //declaring the audio source
        public AudioSource audioJumpPlayer;

        // Start is called before the first frame update
        private void Start()
        {
            //get the components from the audio source from the player
            audioJumpPlayer = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void jumpSound()
        {
            //play the footstep sounds
            audioJumpPlayer.Play();
        }
    }
}