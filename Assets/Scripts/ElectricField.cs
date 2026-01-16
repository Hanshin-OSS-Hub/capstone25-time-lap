using UnityEngine;

public class ElectricField : MonoBehaviour
{
    [Header("초기 상태")]
    public bool startActive = true; // 게임 시작 시 켜져 있는지

    [Header("컴포넌트 연결 (자동 설정됨)")]
    public Collider2D damageCollider; // 닿으면 죽는 범위
    public SpriteRenderer visualRenderer; // 전기 이미지
    public Animator animator; // 전기 애니메이션

    private bool isActive;

    void Awake()
    {
        // 컴포넌트 자동 찾기 (없으면 수동 할당 필요)
        if (damageCollider == null) damageCollider = GetComponent<Collider2D>();
        if (visualRenderer == null) visualRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 초기 상태 적용
        isActive = startActive;
        UpdateState();
    }

    // 발전기 켤 때 호출할 함수
    public void TurnOn()
    {
        isActive = true;
        UpdateState();
    }

    // 발전기 껄 때 호출할 함수
    public void TurnOff()
    {
        isActive = false;
        UpdateState();
    }

    // 상태에 따라 물리/시각 효과 갱신
    void UpdateState()
    {
        // 1. 켜져 있으면 콜라이더 활성화 (닿으면 죽음), 꺼지면 비활성화 (지나갈 수 있음)
        if (damageCollider != null) damageCollider.enabled = isActive;

        // 2. 켜져 있으면 이미지 보임, 꺼지면 안 보임
        if (visualRenderer != null) visualRenderer.enabled = isActive;

        // 3. 애니메이션 제어 (제작한다면 추가하기)
        if (animator != null) animator.SetBool("IsOn", isActive);
    }

    // 충돌 감지 (켜져 있을 때만 작동)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die(); // 플레이어 사망 처리
            }
        }
    }
}