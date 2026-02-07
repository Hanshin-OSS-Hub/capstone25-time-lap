using System.Collections;
using UnityEngine;

[System.Serializable]
public class FallingSpikeSettings
{
    public float fallSpeed = 10f;
    public float shakeDuration = 0.5f;
    public float shakeAmount = 0.1f;
    public float lifetime = 5f;
    public LayerMask groundLayer;
    public float detectionRange = 8f;
    public float detectionWidth = 0.8f;
    public string playerTag = "Player";
}

public class FallingSpike : MonoBehaviour
{
    private float fallSpeed;
    private float shakeDuration;
    private float shakeAmount;
    private float lifetime;
    private LayerMask groundLayer;
    private float detectionRange;
    private float detectionWidth;
    private string playerTag;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    // 내부 로직 변수
    private bool isTriggered = false;
    private bool isFalling = false;
    private float currentShakeTime = 0f;
    private Vector3 originalPos;

    private PlatformSpawner mySpawner;
    public System.Action OnDestroyed;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D col;
    private Coroutine processCoroutine;

    public void Init(PlatformSpawner spawner, FallingSpikeSettings settings)
    {
        this.mySpawner = spawner;

        // 매개변수 settings의 값을 내부 변수에 저장
        this.fallSpeed = settings.fallSpeed;
        this.shakeDuration = settings.shakeDuration;
        this.shakeAmount = settings.shakeAmount;
        this.lifetime = settings.lifetime;
        this.groundLayer = settings.groundLayer;
        this.detectionRange = settings.detectionRange;
        this.detectionWidth = settings.detectionWidth;
        this.playerTag = settings.playerTag;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // 초기 위치 저장
        originalPos = transform.position;

        // 물리 설정
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        if (!isTriggered)
        {
            DetectPlayer();
        }
        else if (isFalling)
        {
            rb.linearVelocity = new Vector2(0, -fallSpeed);
            CheckGroundCollision();
        }
    }

    // 플레이어 감지 (Tag 사용)
    void DetectPlayer()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(detectionWidth, 0.1f), 0f, Vector2.down, detectionRange);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (hit.collider.CompareTag(playerTag))
            {
                StartTriggerSequence();
                break;
            }
        }
    }

    void StartTriggerSequence()
    {
        if (isFrozen || isTriggered) return;

        isTriggered = true;
        originalPos = transform.position;
        processCoroutine = StartCoroutine(ShakeAndFallRoutine());
    }

    IEnumerator ShakeAndFallRoutine()
    {
        while (currentShakeTime < shakeDuration)
        {
            currentShakeTime += Time.deltaTime;
            transform.position = originalPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            yield return null;
        }

        transform.position = originalPos;
        isFalling = true;

        Invoke(nameof(DestroySpike), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
                DestroySpike();
            }
        }
    }

    void CheckGroundCollision()
    {
        if (col == null) return;
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        if (hit.collider != null) DestroySpike();
    }

    public void Freeze(float duration)
    {
        if (!canBeFrozen || isFrozen) return;

        if (processCoroutine != null)
        {
            StopCoroutine(processCoroutine);
            processCoroutine = null;
        }

        isFrozen = true;
        rb.linearVelocity = Vector2.zero;

        if (isTriggered && !isFalling)
        {
            transform.position = originalPos;
        }

        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);

        if (mySpawner != null) mySpawner.PauseSpawning();

        CancelInvoke(nameof(DestroySpike));

        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (!isTriggered)
        {
            // 감지 전이면 아무것도 안 함
        }
        else if (!isFalling)
        {
            processCoroutine = StartCoroutine(ShakeAndFallRoutine());
        }
        else
        {
            Invoke(nameof(DestroySpike), lifetime);
        }

        if (mySpawner != null) mySpawner.ResumeSpawning();
    }

    void DestroySpike()
    {
        if (isFrozen && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }

        OnDestroyed?.Invoke();

        Transform parentTransform = this.transform.parent;
        if (parentTransform != null && parentTransform != this.transform)
            Destroy(parentTransform.gameObject);
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(Unfreeze));
        if (isFrozen && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * (detectionRange / 2), new Vector3(detectionWidth, detectionRange, 0));
    }
}