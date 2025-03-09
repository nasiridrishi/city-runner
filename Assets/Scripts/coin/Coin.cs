using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace coin
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private AudioClip coinSound;

        // Start is called before the first frame update
        /*
        private void Start()
        {
            coinSound = Resources.Load<AudioClip>("Coin");
            if (coinSound == null)
            {
                Debug.Log("Coin not found!");
            }
        }
        */
        
        // Update is called once per frame
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayCoinSound();
                Destroy(gameObject);
            }
            //noop
        }

        private void PlayCoinSound()
        {
            if(coinSound != null)
            {
                AudioSource.PlayClipAtPoint(coinSound, transform.position);
            }
        }

        // private void OnCollisionEnter(Collision other)
        // {
        //     Debug.Log("Collision detected");
        //     //if is player
        //     if (other.gameObject.CompareTag("Player"))
        //     {
        //         //get the player script
        //         var player = other.gameObject.GetComponent<Player.Player>();
        //         //increment the score
        //         player.score++;
        //         //update the UI
        //         player.ui.text = "Score: " + player.score;
        //         //destroy the coin
        //         Destroy(gameObject);
        //     }
        // }
    }
}