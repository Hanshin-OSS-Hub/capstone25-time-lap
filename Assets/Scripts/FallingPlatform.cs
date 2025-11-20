using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private LayerMask groundLayer; // 땅 감지용 레이어

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    // 나를 만든 스포너를 기억하는 변수
    private PlatformSpawner mySpawner;

    public System.Action OnDestroyed;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D col;
    private Coroutine lifetimeCoroutine;

    // 🟢 [추가] 스포너가 생성 직후 호출하여 자신을 등록하는 함수
    public void Init(PlatformSpawner spawner)
    {
        this.mySpawner = spawner;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;

        // 물리 충돌로 밀리는 것 방지 (Kinematic)
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
        DestroyPlatform();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        // 아래로 이동
        rb.linearVelocity = new Vector2(0, -fallSpeed);

        // 땅 감지 (Kinematic은 OnCollisionEnter가 발생 안 하므로 수동 체크)
        CheckGroundCollision();
    }

    void CheckGroundCollision()
    {
        if (col == null) return;

        // 발 밑 감지
        float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, checkDistance, groundLayer);

        if (hit.collider != null)
        {
            DestroyPlatform();
        }
    }

    void StartFalling()
    {
        if (!isFrozen)
            rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    // 🟢 [수정] 스포너 인자를 받을 필요 없이 저장된 mySpawner 사용
    public void Freeze(float duration)
    {
        if (!canBeFrozen || isFrozen) return;

        // 1. 타이머 중지
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        // 2. 상태 변경
        isFrozen = true;
        rb.linearVelocity = Vector2.zero; // 멈춤

        // 3. 시각 효과
        spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);

        // 4. 스포너 멈춤 요청
        if (mySpawner != null)
        {
            mySpawner.PauseSpawning();
        }

        // 5. 일정 시간 후 해제 예약
        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;
        spriteRenderer.color = originalColor;

        StartFalling();

        // 스포너 재개 요청
        if (mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }

        // 수명 타이머 다시 시작
        if (lifetime > 0)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    void DestroyPlatform()
    {
        // 파괴될 때 스포너가 멈춰있다면 풀어줘야 함
        if (isFrozen && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }

        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(Unfreeze));

        // 안전장치: 파괴 시 스포너 재개
        if (isFrozen && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }
    }
}