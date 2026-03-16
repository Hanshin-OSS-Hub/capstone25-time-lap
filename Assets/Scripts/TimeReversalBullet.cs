using UnityEngine;

public class TimeReversalBullet : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 15f; // 날아가는 속도

    [Header("시간 회귀(과거 활성화) 설정")]
    public float effectRadius = 3f;      // 총알 폭발 시 영향을 주는 범위
    public float revertDuration = 5f;    // 과거 상태가 유지되는 시간
    [Tooltip("도착 시 생성할 시각적 이펙트 (과거의 창 프리팹 등)")]
    public GameObject explosionEffectPrefab;

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
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 2. 목표 지점에 거의 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ExplodeAndRevert();
        }
    }

    // 벽이나 장애물에 부딪혔을 때 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🟢 플레이어 본인이나 환풍기 바람(WindArea)은 통과
        if (collision.CompareTag("Player") || collision.GetComponent<WindArea>() != null) return;

        // "Ground" 레이어(벽/바닥)나 기타 오브젝트에 닿으면 목표 지점에 도달 못 했더라도 즉시 터짐
        ExplodeAndRevert();
    }

    // 폭발 및 과거 오브젝트 활성화
    void ExplodeAndRevert()
    {
        // 1. 시각적 이펙트(시간의 창) 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("TimeReversalBullet에 'explosionEffectPrefab'이 할당되지 않았습니다!");
        }

        // 🟢 2. 폭발 범위 내의 모든 콜라이더 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectRadius);

        foreach (Collider2D hit in hits)
        {
            // 감지된 물체에서 PastObject 컴포넌트 찾기
            PastObject pastObj = hit.GetComponent<PastObject>();

            // 만약 콜라이더가 자식에 있다면 부모도 검사해줌
            if (pastObj == null) pastObj = hit.GetComponentInParent<PastObject>();

            if (pastObj != null)
            {
                // 찾았다면 과거 모습 활성화!
                pastObj.ActivatePast(revertDuration);
            }
        }

        // 3. 총알 자신은 파괴
        Destroy(gameObject);
    }

    // 에디터에서 폭발 범위(effectRadius)를 직관적으로 볼 수 있게 그려주는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}