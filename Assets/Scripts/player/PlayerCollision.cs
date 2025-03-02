using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    public Text ui;
    private int score = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Check if the player grabbed the coins
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("coin"))
        {
            score++;
            ui.text = "Score: " + score;
            Destroy(other.gameObject);
        }
    }

    // Check if player hitted obstacles
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacles"))
        {
            ObstaclesDamage obstacles = hit.collider.GetComponent<ObstaclesDamage>();
            if (obstacles != null)
            {
                GetComponent<PlayerHealth>().TakeDamage(obstacles.damage);
            }
        }
    }

}