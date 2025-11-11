using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 추가

public class FallingPlatform : MonoBehaviour
{
    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float lifetime = 10f;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    private PlatformSpawner spawnerReference;

    public System.Action OnDestroyed;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // 🟢 (추가) Lifetime 관리를 위한 Coroutine 참조
    private Coroutine lifetimeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        StartFalling();

        // 🟢 (수정) 일정 시간 후 자동 파괴를 Coroutine으로 시작
        if (lifetime > 0)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    // 🟢 (추가) Lifetime Coroutine
    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        // 수명이 다하면 스스로 파괴
        Destroy(gameObject);
    }

    void Update()
    {
        if (!isFrozen)
        {
            rb.linearVelocity = new Vector2(0, -fallSpeed);
        }
    }

    void StartFalling()
    {
        rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    public void Freeze(float duration, PlatformSpawner spawner)
    {
        if (!canBeFrozen || isFrozen) return;

        this.spawnerReference = spawner;

        // 🟢 (수정) 정지 명령 시, 기존에 진행 중이던 수명(Lifetime) 타이머를 중지
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        isFrozen = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);
        Invoke(nameof(Unfreeze), duration); // 10초 후 Unfreeze 예약
    }

    public void Unfreeze()
    {
        // 🟢 (추가) isFrozen 상태가 아닐 때 중복 호출 방지
        if (!isFrozen) return;

        isFrozen = false;
        rb.isKinematic = false;
        spriteRenderer.color = originalColor;

        StartFalling();

        // 🟢 (수정) 스포너에게 생성 재개 알림
        if (spawnerReference != null)
        {
            spawnerReference.ResumeSpawning();
            spawnerReference = null;
        }

        // 🟢 (추가) 정지 해제 후, 수명 타이머를 다시 시작 (옵션, 필요 없으면 주석 처리)
        if (lifetime > 0)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 🟢 (수정) ResumeSpawning 처리는 OnDestroy에서 일괄 처리
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }

    // 🟢 (핵심 수정) 오브젝트가 파괴되는 모든 순간(수명 만료, 충돌 등) Spawner 재개 처리
    void OnDestroy()
    {
        // 1. Invoke로 예약된 Unfreeze 호출을 취소 (메모리 누수 방지)
        CancelInvoke(nameof(Unfreeze));

        // 2. 오브젝트가 '정지된 상태'에서 파괴된다면 Spawner를 수동으로 재개해야 함.
        if (isFrozen && spawnerReference != null)
        {
            spawnerReference.ResumeSpawning();
        }

        // 3. Lifetime Coroutine이 아직 실행 중이라면 중지
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }

        OnDestroyed?.Invoke();
    }
}