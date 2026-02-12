using UnityEngine;

public class Pogo : MonoBehaviour
{
    [SerializeField] Animator anim;

    public void Activate()
    {
        if (anim != null)
            anim.SetTrigger("Bounce");
    }
}