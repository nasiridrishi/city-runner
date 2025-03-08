using UnityEngine;

//todo improve and bring all player control related members in this class
namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        //spell cooldown
        private static int SPELL_COOLDOWN = 2; //seconds
        private int currentSpellCooldown = 0; //time stamp
        public Animator animator;
        
        private void Update()
        { 
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                setSpell();
            }
            //spell cooldown
        }
         
        private void setSpell()
        {
            //check if spell is on cooldown in timestamp seconds
            int currentTime = (int) Time.time;
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

        public void fireSpell()
        {
            Debug.Log("Spell fired");
        }
        
    }
}