using UnityEngine;

public class GunController : MonoBehaviour
{
    // [유니티 에디터 할당 변수]
    public GameObject bulletPrefab; // 총알 프리팹
    public Transform firePoint;     // 총알이 생성될 위치
    [Tooltip("초당 발사 가능 횟수")]
    public float fireRate = 5f;     // 발사 속도
    public float bulletSpeed = 10f; // 총알 속도

    // [내부 로직 변수]
    private float nextFireTime = 0f; // 다음 발사 가능한 시간

    void Update()
    {
        // 총 회전 함수 호출
        RotateTowardsMouse();

        // 발사 입력(우클릭) 처리
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            // 다음 발사 시간 제어: (현재 시간) + (1초 / 발사 속도)
            nextFireTime = Time.time + 1f / fireRate;

            // 총알 발사 함수 호출
            Shoot();
            Debug.Log("총알 발사");
        }
    }

    // 마우스 위치 방향으로 GunPivot을 회전시키는 함수
    void RotateTowardsMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        // Z축은 카메라 기준으로 설정되어야 2D 월드에서 정확한 좌표를 얻음
        mouseScreenPosition.z = Camera.main.nearClipPlane;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // 현재 위치에서 마우스 위치로 향하는 방향 벡터
        Vector3 direction = mouseWorldPosition - transform.position;

        // Atan2(y, x)로 각도를 라디안으로 계산 및 Deg로 변환
        float angleRadians = Mathf.Atan2(direction.y, direction.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        // GunPivot 오브젝트의 Z축 중심으로 마우스 방향으로 회전 적용
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    // 총알을 생성 및 발사 함수
    void Shoot()
    {
        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 총알의 Rigidbody2D를 가져오기
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 총구가 향하는 방향으로 속도를 부여
            rb.linearVelocity = firePoint.right * bulletSpeed;
        }

        // 총알 생명 주기 관리
        Destroy(bullet, 2f);
    }
}