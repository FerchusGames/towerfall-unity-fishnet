using UnityEngine;

public static class UnityExtensions
{
    public static void SetBool(this Animator[] animators, int id, bool value)
    {
        foreach (Animator animator in animators)
        {
            animator.SetBool(id, value);
        }
    }
    
    public static void SetTrigger(this Animator[] animators, int id)
    {
        foreach (Animator animator in animators)
        {
            animator.SetTrigger(id);
        }
    }
    
    public static void SetFloat(this Animator[] animators, int id, float value)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat(id, value);
        }
    }
    
    public static void SetInteger(this Animator[] animators, int id, int value)
    {
        foreach (Animator animator in animators)
        {
            animator.SetInteger(id, value);
        }
    }
}
