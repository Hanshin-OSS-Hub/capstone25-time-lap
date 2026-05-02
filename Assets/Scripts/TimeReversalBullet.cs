using UnityEngine;

public class TimeReversalBullet : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 15f; // 날아가는 속도

    [Header("시간 회귀(과거 활성화) 설정")]
    public float effectRadius = 2f;      // 총알 폭발 시 영향을 주는 범위
    public float revertDuration = 5f;    // 과거 상태가 유지되는 시간
    [Tooltip("도착 시 생성할 시각적 이펙트 (과거의 창 프리팹 등)")]
    public GameObject explosionEffectPrefab;

    // 내부 변수
    private Vector3 targetPosition;
    private bool hasTarget = false;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    // GunController에서 호출하여 목표 지점을 설정하는 함수
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        targetPosition.z = 0;
        hasTarget = true;

        Vector3 dir = (target - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ExplodeAndRevert();
        }
    }

    // 벽이나 장애물에 부딪혔을 때 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<WindArea>() != null) return;

        ExplodeAndRevert();
    }

    // 폭발 및 과거 오브젝트 활성화
    void ExplodeAndRevert()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 폭발 범위 내의 모든 콜라이더 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectRadius);

        foreach (Collider2D hit in hits)
        {
            PastObject pastObj = hit.GetComponent<PastObject>();
            if (pastObj == null) pastObj = hit.GetComponentInParent<PastObject>();
            if (pastObj != null)
            {
                pastObj.ActivatePast(revertDuration);
            }

            TimeBody timeBody = hit.GetComponent<TimeBody>();
            if (timeBody == null) timeBody = hit.GetComponentInParent<TimeBody>();
            if (timeBody != null)
            {
                timeBody.TimeJump();
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}