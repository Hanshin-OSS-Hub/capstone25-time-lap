using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class VanishingPlatform : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private LayerMask groundLayer;

    [Header("시간 설정")]
    public float disappearDelay = 2.0f; // 사라지기 전 흔들리는 시간
    public float reappearDelay = 3.0f;  // 사라진 후 다시 나타나는 시간

    [Header("떨림 효과")]
    public float shakeAmount = 0.05f;

    [Header("시간정지 설정")]
    [SerializeField] private bool canBeFrozen = true;
    private bool isFrozen = false;

    private Tilemap tilemap;
    private TilemapCollider2D tileCollider;
    private BoxCollider2D boxCollider;

    // 🟢 [핵심] 상태 관리 변수들
    private bool isRunning = false;      // 현재 기믹이 진행 중인가?
    private bool isVanished = false;     // 현재 사라져 있는 상태인가?
    private float currentShakeTime = 0f; // 흔들린 시간 기록용
    private float currentReappearTime = 0f; // 사라진 뒤 흐른 시간 기록용

    private Vector3 originalPos;
    private Color originalColor;

    private Coroutine processCoroutine; // 현재 실행 중인 코루틴 저장

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tileCollider = GetComponent<TilemapCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalPos = transform.position;

        if (tilemap != null) originalColor = tilemap.color;
    }

    void FixedUpdate()
    {
        if (!isFrozen) CheckGroundCollision();
    }

    void CheckGroundCollision()
    {
        Bounds bounds = new Bounds();
        if (tileCollider != null) bounds = tileCollider.bounds;
        else if (boxCollider != null) bounds = boxCollider.bounds;
        else return;

        float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, checkDistance, groundLayer);

        if (hit.collider != null) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFrozen || isRunning) return; // 얼었거나 이미 작동 중이면 무시

        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y < -0.5f) // 위에서 밟음
                {
                    // 처음 밟았을 때 타이머 초기화 후 시작
                    currentShakeTime = 0f;
                    currentReappearTime = 0f;
                    isVanished = false;
                    processCoroutine = StartCoroutine(ProcessRoutine());
                    break;
                }
            }
        }
    }

    // 통합 코루틴 (이어하기 가능 구조)
    IEnumerator ProcessRoutine()
    {
        isRunning = true;

        // --- 1단계: 흔들리기 (사라지기 전) ---
        if (!isVanished)
        {
            while (currentShakeTime < disappearDelay)
            {
                currentShakeTime += Time.deltaTime;

                // 위치 흔들기
                transform.position = originalPos + (Vector3)(Random.insideUnitCircle * shakeAmount);

                // 색상 깜빡임
                if (tilemap != null)
                {
                    float flicker = Mathf.PingPong(Time.time * 10, 1f);
                    tilemap.color = Color.Lerp(originalColor, new Color(1, 0.5f, 0.5f, 1), flicker);
                }

                yield return null;
            }

            // 흔들림 끝 -> 사라짐 처리
            transform.position = originalPos;
            if (tilemap != null)
            {
                Color c = originalColor;
                c.a = 0;
                tilemap.color = c;
            }
            SetColliderState(false);
            isVanished = true; // 이제 사라진 상태임
        }

        // --- 2단계: 대기하기 (다시 나타나기 전) ---
        while (currentReappearTime < reappearDelay)
        {
            currentReappearTime += Time.deltaTime;
            yield return null;
        }

        // --- 3단계: 복구 ---
        ResetPlatform();
    }

    public void Freeze(float duration)
    {
        if (!canBeFrozen || isFrozen) return;

        // 1. 코루틴 강제 중단 (타이머 변수들은 그대로 유지됨 -> 이것이 핵심!)
        if (processCoroutine != null)
        {
            StopCoroutine(processCoroutine);
            processCoroutine = null;
        }

        isFrozen = true;

        // 2. 시각적 정지 효과
        transform.position = originalPos; // 흔들리던거 멈춤 정렬
        if (tilemap != null) tilemap.color = new Color(1f, 1f, 0f, 0.7f); // 노란색

        Invoke(nameof(Unfreeze), duration);
    }

    public void Unfreeze()
    {
        if (!isFrozen) return;

        isFrozen = false;

        // 색상 복구 (사라진 상태였으면 투명하게, 아니면 원래 색으로)
        if (tilemap != null)
        {
            if (isVanished)
            {
                Color c = originalColor;
                c.a = 0;
                tilemap.color = c;
            }
            else
            {
                tilemap.color = originalColor;
            }
        }

        // 중단되었던 작업이 있다면 다시 시작
        if (isRunning)
        {
            processCoroutine = StartCoroutine(ProcessRoutine());
        }
    }

    void ResetPlatform()
    {
        if (tilemap != null) tilemap.color = originalColor;
        SetColliderState(true);
        transform.position = originalPos;

        // 모든 상태 초기화
        isRunning = false;
        isVanished = false;
        currentShakeTime = 0f;
        currentReappearTime = 0f;
        processCoroutine = null;
    }

    void SetColliderState(bool state)
    {
        if (tileCollider != null) tileCollider.enabled = state;
        if (boxCollider != null) boxCollider.enabled = state;
    }
}