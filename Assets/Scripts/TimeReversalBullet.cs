using UnityEngine;

public class TimeReversalBullet : MonoBehaviour
{
    [Header("설정")]
    public float speed = 15f; // 날아가는 속도
    public GameObject timeWindowPrefab; // 도착 시 생성할 '시간의 창' 프리팹

    // 내부 변수
    private Vector3 targetPosition;
    private bool hasTarget = false;

    // 생성되자마자 혹시 모를 무한 생존 방지 (5초 뒤 자동 파괴)
    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    // GunController에서 호출하여 목표 지점을 설정하는 함수
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        targetPosition.z = 0; // 2D 평면 고정
        hasTarget = true;

        // 시각적 효과: 총알이 목표 지점을 바라보게 회전
        Vector3 dir = (target - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (!hasTarget) return;

        // 1. 목표 지점을 향해 이동 (등속 운동)
        // Rigidbody를 쓰지 않고 정확한 지점에 멈추기 위해 MoveTowards 사용
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 2. 목표 지점에 거의 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    // 벽이나 장애물에 부딪혔을 때 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "Ground" 레이어(벽/바닥)에 닿으면 목표 지점에 도달 못 했더라도 즉시 터짐
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
        else if (collision.CompareTag("Player")) return;
        else
        {
            Explode();
        }

        // 추후 특정 오브젝트 및 엔티티에 접촉시 상호작용 추가
        // else if (collision.CompareTag("Obstacle")) { Explode(); }
    }

    // 폭발 및 시간의 창 생성
    void Explode()
    {
        // 시간의 창 프리팹 생성
        if (timeWindowPrefab != null)
        {
            Instantiate(timeWindowPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("TimeReversalBullet에 'TimeWindowPrefab'이 할당되지 않았습니다!");
        }

        // 총알 자신은 파괴
        Destroy(gameObject);
    }
}