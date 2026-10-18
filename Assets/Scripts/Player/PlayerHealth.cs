

using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int invincibleLoopCount= 7;
    [SerializeField] private GameObject dieEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private float deathDelay = 1f;
    [SerializeField] private float disappearDelay = 0.5f;
    [SerializeField] private float deathVelocityMultiplier = 0.6f;
    [SerializeField] private float maxDeathSlideSpeed = 5f;
    [SerializeField] private float deathDrag = 1.5f;

    public int MaxHealth => maxHealth;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    public int CurrentHealth { get; private set; }
    public bool IsInvincible { get; private set; }

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private PlayerAnimation anim;
    private Rigidbody2D rb;
    private float defaultDrag;
    private bool isDying;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<PlayerAnimation>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            defaultDrag = rb.drag;
    }

    private void Start() => OnHealthChanged?.Invoke(CurrentHealth);

    public void TakeDamage(int damage)
    {
        if (CurrentHealth <= 0 || IsInvincible || isDying || playerController.IsWinning)
            return;

        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        { Die(); return; }

        SoundManager.Instance?.PlaySound(hitSound);
        playerController.ApplyKnockback(4f);
        anim?.PlayHitAnimation();
        StartCoroutine(InvincibilityRoutine());
    }

    public void Die()
    {
        if (isDying)
            return;

        isDying = true;
        CurrentHealth = 0;
        OnHealthChanged?.Invoke(CurrentHealth);
        playerController.enabled = false;
        if (rb != null)
        {
            float dampedX = rb.velocity.x * deathVelocityMultiplier;
            float clampedX = Mathf.Clamp(dampedX, -maxDeathSlideSpeed, maxDeathSlideSpeed);
            rb.velocity = new Vector2(clampedX, rb.velocity.y);
            rb.drag = deathDrag;
        }
        anim?.PlayDieAnimation();
        SoundManager.Instance?.PlaySound(dieSound);
        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        anim?.PlayDisappearAnimation();
        yield return new WaitForSeconds(disappearDelay);
        gameObject.SetActive(false);
        OnDeath?.Invoke();
        RespawnManager.Instance?.HandlePlayerDeath();
    }

    public void ActivePlayer()
    {
        gameObject.SetActive(true);
        if (playerController != null)
            playerController.enabled = true;
        if (anim != null)
        {
            anim.enabled = true;
            anim.EnableAnimator();
        }
    }

    public void Heal(int healAmount)
    {
        CurrentHealth += healAmount;
    }

    public void Reset()
    {
        CurrentHealth = maxHealth;
        IsInvincible = false;
        isDying = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.drag = defaultDrag;
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        for (int i = invincibleLoopCount; i > 0; i--)
        {
            SetSpriteAlpha(0.1f * i);
            yield return new WaitForSeconds(0.05f * i);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(0.05f * i);

        }
        IsInvincible = false;
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            TakeDamage(1);
    }
}
