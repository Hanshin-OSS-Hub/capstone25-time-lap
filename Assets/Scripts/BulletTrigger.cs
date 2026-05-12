using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public GameObject explosionEffectPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<WindArea>() != null) return;

        // 1. Platform 감지
        FallingPlatform platform = collision.GetComponent<FallingPlatform>();
        MovingPlatform movingPlatform = collision.GetComponent<MovingPlatform>();
        RotatingFan rotatingFan = collision.GetComponent<RotatingFan>();
        Updraft updraft = collision.GetComponent<Updraft>();
        Generator generator = collision.GetComponent<Generator>();
        FallingSpike spike = collision.GetComponent<FallingSpike>();
        VanishingPlatform vanishing = collision.GetComponent<VanishingPlatform>();
        MathPendulum pendulum = collision.GetComponent<MathPendulum>();

        PlatformSpawner spawner = collision.GetComponent<PlatformSpawner>();

        if (platform != null) // 떨어지는 플렛폼
        {
            Debug.Log("시간 정지 총알 명중!");
            platform.Freeze(10f);
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (movingPlatform != null) // 움직이는 플렛폼
        {
            Debug.Log("시간 정지 총알 명중!");
            movingPlatform.Freeze(5f);
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (rotatingFan != null) // 환풍기
        {
            Debug.Log("환풍기 명중!");
            rotatingFan.Freeze(5f);
            ShowEffectAndDestroy();
            Destroy(gameObject);
        }
        else if (updraft != null) // 상승기류 환풍기
        {
            Debug.Log("상승 환풍기 명중!");
            updraft.Freeze(5f);
            ShowEffectAndDestroy();
        }
        else if (generator != null) // 발전기
        {
            Debug.Log("발전기 명중!");
            generator.Freeze(5f);
            ShowEffectAndDestroy();
        }
        else if (spike != null) // 떨어지는 가시
        {
            Debug.Log("가시 명중!");
            spike.Freeze(5f);
            ShowEffectAndDestroy();
        }
        else if (vanishing != null) // 사라지는 플랫폼
        {
            Debug.Log("사라지는 발판 명중!");
            vanishing.Freeze(5f);
            ShowEffectAndDestroy();
        }
        else if (pendulum != null) // 팬듈럼 플랫폼
        {
            Debug.Log("진자 명중!");
            pendulum.FreezeTime();
            ShowEffectAndDestroy();
        }
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