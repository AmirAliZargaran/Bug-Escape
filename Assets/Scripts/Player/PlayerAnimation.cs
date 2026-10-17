

using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerController controller;

    private static readonly int IsWallSlidingHash = Animator.StringToHash("isWallSliding");
    private static readonly int XVelocityHash = Animator.StringToHash("xVelocity");
    private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int IsDashingHash = Animator.StringToHash("isDashing");
    private static readonly int IsWallHash = Animator.StringToHash("isWall");
    private static readonly int TakeDamageHash = Animator.StringToHash("takeDamage");
    private static readonly int DoubleJumpHash = Animator.StringToHash("doubleJump");

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (controller.IsWinning)
        { anim.enabled = false; return; }

        Vector2 platformVelocity = controller.GetActivePlatformVelocity();
        float relativeXVelocity = rb.velocity.x - platformVelocity.x;
        float relativeYVelocity = rb.velocity.y - platformVelocity.y;

        anim.SetFloat(XVelocityHash, Mathf.Abs(relativeXVelocity));
        anim.SetFloat(YVelocityHash, relativeYVelocity);
        anim.SetBool(IsGroundedHash, controller.IsGrounded);
        anim.SetBool(IsDashingHash, controller.IsDashing);
        anim.SetBool(IsWallHash, controller.IsWall);
        anim.SetBool(IsWallSlidingHash, controller.IsWallSliding);

    }

    public void PlayHitAnimation() => anim.SetTrigger(TakeDamageHash);

    public void PlayDieAnimation() => anim.SetTrigger(Animator.StringToHash("die"));

    public void PlayDisappearAnimation() => anim.SetTrigger(Animator.StringToHash("disappear"));

    public void EnableAnimator() => anim.enabled = true;

    public void PlayDoubleJumpAnimation()
    {
        anim.SetTrigger(DoubleJumpHash);
    }
}
