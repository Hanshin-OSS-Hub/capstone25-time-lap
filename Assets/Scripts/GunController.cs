using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("시간 정지 총알 설정 (좌클릭)")]
    public GameObject freezeBulletPrefab; // 기존 bulletPrefab (이름 변경됨)
    public float fireRate = 5f;
    public float bulletSpeed = 10f;

    [Header("시간 역행 총알 설정 (우클릭)")]
    public GameObject timeBulletPrefab;   // ⭐ 새로 추가된 프리팹 슬롯
    public float timeBulletCooldown = 2.0f; // 특수 총알 쿨타임

    [Header("공통 설정")]
    public Transform firePoint;

    // 내부 변수
    private float nextFireTime = 0f;
    private float nextTimeBulletTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. 정확한 마우스 월드 좌표 계산 (핵심 수정)
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 2. 캐릭터(Player) 좌우 반전
        FlipCharacter(mouseWorldPos);

        // 3. 총(Hand/Gun) 회전
        RotateGun(mouseWorldPos);

        // 4. 좌클릭: 시간 정지 총알 발사
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Shoot(mouseWorldPos);
        }

        // 5. 우클릭: 시간 역행 총알 발사 (⭐ 새로 추가됨)
        if (Input.GetMouseButtonDown(1) && Time.time >= nextTimeBulletTime)
        {
            nextTimeBulletTime = Time.time + timeBulletCooldown;
            ShootTimeBullet(mouseWorldPos);
        }
    }

    // ⭐ 핵심 수정 1: 마우스 좌표의 Z값을 카메라 깊이에 맞춰 보정
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        // 카메라와 0,0,0 평면 사이의 거리만큼 Z를 설정해야 정확한 월드 좌표가 나옵니다.
        mouseScreenPos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    // ⭐ 핵심 수정 2: 부모(Player)를 통째로 뒤집기
    void FlipCharacter(Vector3 mousePos)
    {
        Transform playerTransform = transform.parent;
        if (playerTransform != null)
        {
            // 마우스가 오른쪽에 있으면 정방향 (1)
            if (mousePos.x > playerTransform.position.x)
            {
                playerTransform.localScale = new Vector3(1, 1, 1);
            }
            // 마우스가 왼쪽에 있으면 좌우반전 (-1)
            else
            {
                playerTransform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    // ⭐ 핵심 수정 3: 뒤집힌 부모에 맞춰 총 회전 각도 보정
    void RotateGun(Vector3 mousePos)
    {
        Vector3 direction = mousePos - transform.position;

        // 부모(Player)가 뒤집혀 있다면(-1), 월드 좌표계의 방향 벡터를 로컬 좌표계로 변환해야 합니다.
        // 부모 X가 -1이면, 월드에서 '왼쪽'은 로컬에서 '오른쪽'입니다.
        if (transform.parent.localScale.x < 0)
        {
            direction.x = -direction.x; // X축 방향을 뒤집어서 계산
        }

        // 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 회전 적용
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // ⭐ 핵심 수정 4: 총구에서 마우스로 향하는 '진짜' 방향으로 발사
    void Shoot(Vector3 mousePos)
    {
        // 총구에서 마우스까지의 방향 벡터 (정규화)
        Vector2 shootDirection = (mousePos - firePoint.position).normalized;

        GameObject bullet = Instantiate(freezeBulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 총의 회전값이나 부모의 반전 여부와 상관없이, 계산된 월드 방향으로 날려보냅니다.
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        Destroy(bullet, 2f);
    }

    // ⭐ 시간 역행 총알 발사 (새로 추가된 함수)
    void ShootTimeBullet(Vector3 mousePos)
    {
        if (timeBulletPrefab == null)
        {
            Debug.LogWarning("Time Bullet Prefab이 할당되지 않았습니다!");
            return;
        }

        // 총알 생성
        GameObject bullet = Instantiate(timeBulletPrefab, firePoint.position, Quaternion.identity);

        // TimeReversalBullet 스크립트를 가져와서 목표 지점 설정
        TimeReversalBullet timeBullet = bullet.GetComponent<TimeReversalBullet>();
        if (timeBullet != null)
        {
            timeBullet.SetTarget(mousePos);
        }
    }
}