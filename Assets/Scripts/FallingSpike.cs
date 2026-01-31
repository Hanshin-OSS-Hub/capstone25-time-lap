using System.Collections;
using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRange = 8f;   // 아래로 감지할 거리
    [SerializeField] private float detectionWidth = 0.8f; // 감지할 폭
    [SerializeField] private string playerTag = "Player";

    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    // 내부 상태 변수
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

    public void Init(PlatformSpawner spawner)
    {
        this.mySpawner = spawner;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        originalPos = transform.position;

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

    void DetectPlayer()
    {
        // BoxCastAll을 사용하여 경로상에 있는 '모든' 물체를 감지
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(detectionWidth, 0.1f), 0f, Vector2.down, detectionRange);

        // 감지된 물체들을 하나씩 확인
        foreach (RaycastHit2D hit in hits)
        {
            // 자기 자신은 감지 제외
            if (hit.collider.gameObject == gameObject) continue;

            // 태그 확인 ("Player")
            if (hit.collider.CompareTag(playerTag))
            {
                StartTriggerSequence();
                break; // 플레이어를 찾았으면 루프 종료
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
        // Tag로 비교
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