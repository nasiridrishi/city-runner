using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace coin
{
    public class Coin : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            //noop
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