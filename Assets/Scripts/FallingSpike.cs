using System.Collections;
using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    // 스포너 연결
    private PlatformSpawner mySpawner;
    public System.Action OnDestroyed;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // 가시는 보통 스프라이트
    private Color originalColor;
    private Collider2D col;
    private Coroutine lifetimeCoroutine;

    // 초기화 (스포너가 호출)
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

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        StartFalling();

        if (lifetime > 0)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        DestroySpike();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        // 아래로 낙하
        rb.linearVelocity = new Vector2(0, -fallSpeed);

        // 땅 감지 (땅에 닿으면 사라짐)
        CheckGroundCollision();
    }

    // 플레이어 충돌 감지 (Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어와 닿았는지 확인
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 플레이어 사망 처리
                player.Die();

                DestroySpike();
            }
        }
    }

    void CheckGroundCollision()
    {
        if (col == null) return;

        // 박스 캐스트로 땅 감지
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null)
        {
            // 땅에 닿으면 파괴
            DestroySpike();
        }
    }

    void StartFalling()
    {
        if (!isFrozen)
            rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    // 시간 정지 (FallingPlatform과 동일 로직)
    public void Freeze(float duration)
    {
        if (!canBeFrozen || isFrozen) return;

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        isFrozen = true;
        rb.linearVelocity = Vector2.zero;

        // 색상 변경 (노란색 반투명)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);
        }

        // 스포너 멈춤
        if (mySpawner != null) mySpawner.PauseSpawning();

        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        StartFalling();

        if (mySpawner != null) mySpawner.ResumeSpawning();

        if (lifetime > 0)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    void DestroySpike()
    {
        Transform parentTransform = this.transform.parent;

        if (isFrozen && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }

        OnDestroyed?.Invoke();

        // 부모가 있으면 부모까지 삭제 (구조에 따라 다름)
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
}