using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace coin
{
    public class Coin : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayCoinSound();
                Destroy(gameObject);
            }
        }

        private void PlayCoinSound()
        {
            SoundManager.instance.soundCoinCollect();
        }
    }
}