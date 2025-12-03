using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float lifetime = 20f; // 왕복이면 수명을 길게 잡으세요

    [Tooltip("이동 방향 (1, 0)은 오른쪽, (-1, 0)은 왼쪽")]
    [SerializeField] private Vector2 moveDirection = Vector2.right;

    [SerializeField] private LayerMask obstacleLayer; // 벽/장애물 감지용 레이어

    [Header("왕복 운동 설정")]
    [Tooltip("체크하면 벽에 닿았을 때 파괴되지 않고 뒤로 돌아갑니다.")]
    [SerializeField] private bool isRoundTrip = false;

    [Tooltip("0보다 크면, 벽에 닿지 않아도 이 시간마다 방향을 바꿉니다.")]
    [SerializeField] private float autoTurnTime = 0f;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    // 내부 변수
    private PlatformSpawner mySpawner;
    public System.Action OnDestroyed;
    private Rigidbody2D rb;
    private TilemapRenderer tilemapRenderer;
    private Color originalColor;
    private Collider2D col;
    private Coroutine lifetimeCoroutine;
    private float turnTimer = 0f; // 자동 방향 전환용 타이머

    public void Init(PlatformSpawner spawner)
    {
        this.mySpawner = spawner;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        col = GetComponent<Collider2D>();

        if (tilemapRenderer != null)
        {
            originalColor = tilemapRenderer.material.color;
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        // 시작 시 이동 방향 정규화 (길이가 1인 벡터로 만듦)
        moveDirection = moveDirection.normalized;

        StartMoving();

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

        // 1. 이동 처리
        rb.linearVelocity = moveDirection * moveSpeed;

        // 2. 벽 감지 (충돌 시 처리)
        CheckWallCollision();

        // 3. 시간 기반 방향 전환 (옵션)
        if (isRoundTrip && autoTurnTime > 0)
        {
            turnTimer += Time.fixedDeltaTime;
            if (turnTimer >= autoTurnTime)
            {
                TurnAround();
                turnTimer = 0f; // 타이머 리셋
            }
        }
    }

    void CheckWallCollision()
    {
        if (col == null) return;

        float checkDistance = 0.1f;

        // 이동하는 방향으로 레이캐스트 발사
        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center,
            col.bounds.size,
            0f,
            moveDirection,
            checkDistance,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            // ⭐ 왕복 모드인지 확인 ⭐
            if (isRoundTrip)
            {
                // 벽에 닿으면 방향을 반대로 뒤집음
                TurnAround();
            }
            else
            {
                // 왕복이 아니면 그냥 파괴 (기존 로직)
                DestroyPlatform();
            }
        }
    }

    // ⭐ 방향을 반대로 바꾸는 함수 ⭐
    void TurnAround()
    {
        // 방향 벡터를 반대로 뒤집음 (오른쪽 -> 왼쪽)
        moveDirection = moveDirection * -1;

        // 즉시 속도 적용 (미끄러짐 방지)
        if (!isFrozen)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }

        // 방향 전환 시 타이머 초기화 (시간 기반 전환을 쓰고 있을 경우 꼬임 방지)
        turnTimer = 0f;
    }

    void StartMoving()
    {
        if (!isFrozen)
            rb.linearVelocity = moveDirection * moveSpeed;
    }

    // ... (Freeze, Unfreeze, DestroyPlatform 등 나머지는 기존과 동일) ...
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

        if (tilemapRenderer != null)
            tilemapRenderer.material.color = new Color(1f, 1f, 0f, 0.7f);

        if (mySpawner != null) mySpawner.PauseSpawning();

        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;
        if (tilemapRenderer != null) tilemapRenderer.material.color = originalColor;

        StartMoving();

        if (mySpawner != null) mySpawner.ResumeSpawning();

        if (lifetime > 0)
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    void DestroyPlatform()
    {
        Transform parentTransform = this.transform.parent;

        if (isFrozen && mySpawner != null) mySpawner.ResumeSpawning();

        OnDestroyed?.Invoke();

        if (parentTransform != null) Destroy(parentTransform.gameObject);
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(Unfreeze));
        if (isFrozen && mySpawner != null) mySpawner.ResumeSpawning();
    }
}