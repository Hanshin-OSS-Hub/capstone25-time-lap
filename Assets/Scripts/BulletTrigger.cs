using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public GameObject explosionEffectPrefab; // 충돌 이펙트 프리팹

    // 충돌이 발생시 함수 호출
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("총알 충돌");
        // 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            // 파티클 시스템 프리팹 생성
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        // 충돌시 총알 파괴
        Destroy(gameObject);
    }
}