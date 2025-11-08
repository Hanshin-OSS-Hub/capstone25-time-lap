using UnityEngine;


public class FallingPlatform : MonoBehaviour
{
    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float lifetime = 10f;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    public System.Action OnDestroyed;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // 낙하 시작
        StartFalling();

        // 일정 시간 후 자동 파괴
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }

    void Update()
    {
        // 정지 상태가 아니면 계속 낙하
        if (!isFrozen)
        {
            rb.linearVelocity = new Vector2(0, -fallSpeed);
        }
    }

    void StartFalling()
    {
        rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    // 시간정지 기능 (나중에 레이저와 연동)
    public void Freeze(float duration)
    {
        if (!canBeFrozen) return;

        isFrozen = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // 시각 효과 (노란색 반투명)
        spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);
        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
{
    isFrozen = false;
    rb.isKinematic = false;
    spriteRenderer.color = originalColor;

    // 낙하 재개
    StartFalling();
}

void OnCollisionEnter2D(Collision2D collision)
{
    // 바닥에 닿으면 파괴
    if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
    {
        OnDestroyed?.Invoke(); // 이벤트 호출
        Destroy(gameObject);
    }
}

// OnDestroy 메서드 추가 (수명 종료 시에도 이벤트 호출)
void OnDestroy()
{
    OnDestroyed?.Invoke();
}
}