using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorDriver : MonoBehaviour
{
    public string moveSpeedParam = "MoveSpeed";
    public string attackTriggerParam = "Attack";
    public string jumpTriggerParam = "Jump";

    [Header("Smoothing")]
    public float speedDampTime = 0.04f;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetMoveSpeed(float inputMagnitude)
    {
        // inputMagnitude: 0~1
        anim.SetFloat(moveSpeedParam, inputMagnitude, speedDampTime, Time.deltaTime);
    }

    public void TriggerAttack()
    {
        anim.SetTrigger(attackTriggerParam);
    }

    public void TriggerJump()
    {
        anim.SetTrigger(jumpTriggerParam);
    }

    public void StartClimbing()
    {
        anim.SetBool("Climb", true);
    }

    public void SetClimbingSpeed(float speed)
    {
        anim.speed = speed;
    }

    public void LeaveClimb()
    {
        anim.SetBool("Climb", false);
        anim.speed = 1f;
    }

    public void Finish()
    {
        anim.SetTrigger("Finish");
    }
}
