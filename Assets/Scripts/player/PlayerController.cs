using UnityEngine;

//todo improve and bring all player control related members in this class
namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public Animator animator;
        
        private void Update()
        { 
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                setMagic();
            }
        }
         
        private void setMagic()
        {
            animator.SetTrigger("Magic");
        }
        
        private void stopMagic()
        {
            animator.ResetTrigger("Magic");
        }
        
    }
}