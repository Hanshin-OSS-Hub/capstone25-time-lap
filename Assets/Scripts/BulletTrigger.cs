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
        RotatingFan rotatingFan = collision.GetComponent<RotatingFan>();
        Updraft updraft = collision.GetComponent<Updraft>();
        Generator generator = collision.GetComponent<Generator>();
        FallingSpike spike = collision.GetComponent<FallingSpike>();
        VanishingPlatform vanishing = collision.GetComponent<VanishingPlatform>();

        if (platform != null) // 떨어지는 플렛폼
        {
            Debug.Log("시간 정지 총알 명중!");

            // 플랫폼에게 10초간 얼라고 명령 (스포너 처리는 플랫폼이 알아서 함)
            platform.Freeze(10f);

            // 이펙트 생성 및 총알 파괴
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (movingPlatform != null) // 움직이는 플렛폼
        {
            Debug.Log("시간 정지 총알 명중!");
            // 플랫폼에게 10초간 얼라고 명령 (스포너 처리는 플랫폼이 알아서 함)
            movingPlatform.Freeze(10f);
            // 이펙트 생성 및 총알 파괴
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (rotatingFan != null) // 환풍기
        {
            Debug.Log("환풍기 명중!");
            rotatingFan.Freeze(10f);
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (updraft != null) // 상승기류 환풍기
        {
            Debug.Log("상승 환풍기 명중!");
            updraft.Freeze(10f); // 10초간 정지
            ShowEffectAndDestroy();
        }
        else if (generator != null) // 발전기
        {
            Debug.Log("발전기 명중!");
            generator.Freeze(10f); // 10초간 상호작용 불가
            ShowEffectAndDestroy();
        }
        else if (spike != null) // 떨어지는 가시
        {
            Debug.Log("가시 명중!");
            spike.Freeze(10f);
            ShowEffectAndDestroy();
        }
        else if (vanishing != null)
        {
            Debug.Log("사라지는 발판 명중!");
            vanishing.Freeze(10f); // 10초간 정지 및 초기화
            ShowEffectAndDestroy();
        }
        // 2. 벽이나 땅에 닿았을 때도 총알 삭제 (Ground 레이어 확인 필요)
        else if (collision.CompareTag("Player")) return;
        else
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