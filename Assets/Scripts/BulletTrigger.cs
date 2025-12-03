using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public GameObject explosionEffectPrefab; // 충돌 이펙트 프리팹

    // 🟢 [핵심 수정] 물리 충돌(Collision) 대신 트리거(Trigger) 사용
    // 필독: 총알 Prefab의 Collider2D 컴포넌트에서 'Is Trigger'를 체크해야 합니다!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Platform 감지
        FallingPlatform platform = collision.GetComponent<FallingPlatform>();
        MovingPlatform movingPlatform = collision.GetComponent<MovingPlatform>();

        if (platform != null)
        {
            Debug.Log("시간 정지 총알 명중!");

            // 플랫폼에게 10초간 얼라고 명령 (스포너 처리는 플랫폼이 알아서 함)
            platform.Freeze(10f);

            // 이펙트 생성 및 총알 파괴
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (movingPlatform != null)
        {
            Debug.Log("시간 정지 총알 명중!");
            // 플랫폼에게 10초간 얼라고 명령 (스포너 처리는 플랫폼이 알아서 함)
            movingPlatform.Freeze(10f);
            // 이펙트 생성 및 총알 파괴
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        // 2. 벽이나 땅에 닿았을 때도 총알 삭제 (Ground 레이어 확인 필요)
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
    }

    void ShowEffectAndDestroy()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}