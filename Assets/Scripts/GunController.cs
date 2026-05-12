using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    [Header("시간 정지 총알 설정 (좌클릭)")]
    public GameObject freezeBulletPrefab;
    public float fireRate = 5f;
    public float bulletSpeed = 10f;
    public int maxBullets = 10;
    private int currentBullets;


    [Header("시간 역행 총알 설정 (우클릭)")]
    public GameObject timeBulletPrefab;
    public float timeBulletCooldown = 2.0f;

    [Header("공통 설정")]
    public Transform firePoint;

    [Header("사운드")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip timeShootClip;

    // 내부 변수
    private float nextFireTime = 0f;
    private float nextTimeBulletTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        currentBullets = maxBullets;
        UpdateUI();
    }

    void Update()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        FlipCharacter(mouseWorldPos);

        RotateGun(mouseWorldPos);

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Shoot(mouseWorldPos);
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= nextTimeBulletTime)
        {
            nextTimeBulletTime = Time.time + timeBulletCooldown;
            ShootTimeBullet(mouseWorldPos);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    void FlipCharacter(Vector3 mousePos)
    {
        Transform playerTransform = transform.parent;
        if (playerTransform != null)
        {
            if (mousePos.x > playerTransform.position.x)
            {
                playerTransform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                playerTransform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    void RotateGun(Vector3 mousePos)
    {
        Vector3 direction = mousePos - transform.position;

        if (transform.parent.localScale.x < 0)
        {
            direction.x = -direction.x; 
        }

        // 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 회전 적용
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot(Vector3 mousePos)
    {
        if (currentBullets <= 0) return;

        Vector2 shootDirection = (mousePos - firePoint.position).normalized;

        GameObject bullet = Instantiate(freezeBulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        currentBullets--;
        UpdateUI();

        Debug.Log($"빵! 남은 총알: {currentBullets} / {maxBullets}");

        Destroy(bullet, 2f);
    }

    void ShootTimeBullet(Vector3 mousePos)
    {
        if (timeBulletPrefab == null)
        {
            Debug.LogWarning("Time Bullet Prefab이 할당되지 않았습니다!");
            return;
        }

        GameObject bullet = Instantiate(timeBulletPrefab, firePoint.position, Quaternion.identity);

        TimeReversalBullet timeBullet = bullet.GetComponent<TimeReversalBullet>();
        if (timeBullet != null)
        {
            timeBullet.SetTarget(mousePos);
        }
        if (audioSource != null && timeShootClip != null)
        {
            audioSource.PlayOneShot(timeShootClip);
        }
    }
    void UpdateUI()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateBulletUI(currentBullets, maxBullets);
        }
    }
}