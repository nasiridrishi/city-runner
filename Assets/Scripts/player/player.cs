using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class player : MonoBehaviour
    {
        public Text ui;
        public int score = 0;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("coin"))
            {
                score++;
                ui.text = "Score: " + score;
                Destroy(other.gameObject);
            }
        }
    }
}