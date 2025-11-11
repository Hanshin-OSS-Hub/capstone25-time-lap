using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public GameObject explosionEffectPrefab; // 충돌 이펙트 프리팹

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("총알 충돌");

        // 충돌한 오브젝트에서 FallingPlatform 스크립트를 가져옴
        FallingPlatform platform = collision.gameObject.GetComponent<FallingPlatform>();

        if (platform != null)
        {
            // 1. 씬에서 PlatformSpawner 인스턴스를 찾습니다. (씬에 스포너가 하나만 있다고 가정)
            PlatformSpawner spawner = FindObjectOfType<PlatformSpawner>();

            if (spawner != null)
            {
                // 2. Spawner에게 생성 중단 요청 (스포너는 즉시 생성 중단)
                spawner.PauseSpawning();

                // 3. FallingPlatform에게 10초 정지 명령을 내리고, 정지 해제 시 사용할 Spawner 참조를 전달
                // 10초 후 Spawner 재개는 FallingPlatform이 책임집니다.
                platform.Freeze(10f, spawner);
            }
            else
            {
                // 스포너를 찾지 못했더라도 플랫폼만 정지 (스포너 기능 없이)
                platform.Freeze(10f, null);
            }
        }

        // 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 충돌시 총알 파괴
        Destroy(gameObject);
    }
}