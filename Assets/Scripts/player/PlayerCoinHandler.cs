using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerCoinHandler : MonoBehaviour
    {
        public Text ui;
        public int score = 0;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Collision detected with : " + other.tag);
            if (other.tag.Equals("coin"))
            {
                score++;
                ui.text = "Score: " + score;
                Destroy(other.gameObject);
            }
        }
    }
}