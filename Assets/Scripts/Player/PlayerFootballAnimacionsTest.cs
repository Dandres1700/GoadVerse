using UnityEngine;

public class PlayerFootballAnimationsTest : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            animator.SetTrigger("Shoot");
        }
    }
}