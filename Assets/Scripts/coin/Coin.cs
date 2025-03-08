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
            transform.Rotate(0, 0, 2);
        }
    }
}