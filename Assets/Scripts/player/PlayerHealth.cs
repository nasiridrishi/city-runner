using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public float Health;
    private float currentHealth;
    public Animator animator;
    private PlayerMovement playerMovement;

    public float invulnerabilityLength;
    private float invulnerabilityCounter;
    private bool invulnerable;


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = Health;
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (invulnerabilityCounter > 0)
        {
            invulnerabilityCounter -= Time.deltaTime;
        }
    }

    // Character Taken Damage Logic
    public void TakeDamage(float dmg)
    {
        // Skip the TakeDamage function if player just hitted.
        if (invulnerable == true)
        {
            return;
        }

        currentHealth -= dmg;

        // If player health is above 0 and is not invulnerable stop movement and invulnerable
        if (currentHealth > 0 && invulnerabilityCounter <= 0)
        {
            invulnerabilityCounter = invulnerabilityLength;
            StartCoroutine(StopPlayer(invulnerabilityCounter));
            StartCoroutine(invulnerability());
        }

        // If player health is equal or below 0
        else if (currentHealth <= 0)
        {
            animator.SetTrigger("Dead");
            StartCoroutine(StopPlayer(2));
            Invoke("playerRespawn", 2.0f);
        }

    }

    // Set animation for player death, reset health and call respawn function
    public void playerRespawn()
    {
        animator.ResetTrigger("Dead");
        animator.Play("Idle");
        currentHealth = Health;
        GetComponent<PlayerRespawn>().respawn();
    }

    // Disable movement script when player dead
    private IEnumerator StopPlayer(float seconds)
    {
        playerMovement.controller.enabled = false;
        yield return new WaitForSeconds(seconds);
        playerMovement.controller.enabled = true;
    }

    private IEnumerator invulnerability()
    {
        invulnerable = true;
        animator.SetTrigger("Hitted");
        yield return new WaitForSeconds(invulnerabilityCounter);
        invulnerable = false;
        animator.ResetTrigger("Hitted");
        animator.Play("Idle");
    }
}