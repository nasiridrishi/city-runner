using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] public Transform spawnPoint;

    // Respawn player to original spawn point
    public void respawn()
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        GetComponent<Animator>().SetFloat("MovementSpeed", 0);
    }
}