using System;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        //spell cooldown
        private static int SPELL_COOLDOWN = 2; //seconds
        private int currentSpellCooldown = 0; //time stamp
        public Animator animator;

        public GameObject magic;
        public Transform magicStartPos;
        private Rigidbody rgb;

        private void Start()
        {
            rgb = magic.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Mouse0)) setSpell();
            //spell cooldown
        }

        private void setSpell()
        {
            //check if spell is on cooldown in timestamp seconds
            var currentTime = (int)Time.time;
            if (currentSpellCooldown + SPELL_COOLDOWN < currentTime)
            {
                animator.SetTrigger("Spell");
                currentSpellCooldown = currentTime;
            }
        }

        private void stopSpell()
        {
            animator.ResetTrigger("Spell");
        }

        public void castMagic()
        {
            magic.transform.position = magicStartPos.position;
            magic.SetActive(true);
            rgb.AddForce(transform.forward * 1000);
            Invoke(nameof(destroyMagic), 3); // Backup deactivation after 3 seconds
        }

        // Changed to public so it can be called from ObstacleWall
        public void destroyMagic()
        {
            // Cancel any pending invoke calls to avoid double-deactivation
            CancelInvoke(nameof(destroyMagic));

            // Reset magic state
            if (rgb != null)
            {
                rgb.linearVelocity = Vector3.zero;
                rgb.angularVelocity = Vector3.zero;
            }

            magic.SetActive(false);
        }
    }
}