using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class FallingPlatformSettings
{
    public float fallSpeed = 3f;
    public float lifetime = 10f;
    public LayerMask groundLayer;

    public bool pauseSpawnerOnFreeze = false;
}

public class FallingPlatform : MonoBehaviour
{
    private float fallSpeed;
    private float lifetime;
    private LayerMask groundLayer;

    private bool pauseSpawnerOnFreeze;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    private PlatformSpawner mySpawner;
    public System.Action OnDestroyed;

    private Rigidbody2D rb;
    private TilemapRenderer tilemapRenderer;
    private Color originalColor;
    private Collider2D col;
    private Coroutine lifetimeCoroutine;


    // 🟢 [추가] 스포너가 생성 직후 호출하여 자신을 등록하는 함수
    public void Init(PlatformSpawner spawner, FallingPlatformSettings settings)
    {
        this.mySpawner = spawner;

        // 스포너에서 받은 설정 적용
        this.fallSpeed = settings.fallSpeed;
        this.lifetime = settings.lifetime;
        this.groundLayer = settings.groundLayer;

        this.pauseSpawnerOnFreeze = settings.pauseSpawnerOnFreeze;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        col = GetComponent<Collider2D>();

        if (tilemapRenderer != null) originalColor = tilemapRenderer.material.color;

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
        if (tilemapRenderer != null)
        {
            // 렌더러의 material 인스턴스를 가져와 색상을 변경합니다.
            tilemapRenderer.material.color = new Color(1f, 1f, 0f, 0.7f);
        }

        // 4. 스포너 멈춤 요청
        if (pauseSpawnerOnFreeze && mySpawner != null)
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

        if (tilemapRenderer != null)
        {
            tilemapRenderer.material.color = originalColor;
        }

        StartFalling();

        // 스포너 재개 요청
        if (pauseSpawnerOnFreeze && mySpawner != null)
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
        Transform parentTransform = this.transform.parent;
        // 파괴될 때 스포너가 멈춰있다면 풀어줘야 함
        if (isFrozen && pauseSpawnerOnFreeze && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }

        OnDestroyed?.Invoke();
        Destroy(parentTransform.gameObject);    
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(Unfreeze));

        // 안전장치: 파괴 시 스포너 재개
        if (isFrozen && pauseSpawnerOnFreeze && mySpawner != null)
        {
            mySpawner.ResumeSpawning();
        }
    }
}