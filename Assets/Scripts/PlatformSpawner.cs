using UnityEngine;
using System.Collections;

public class PlatformSpawner : MonoBehaviour
{
    [Header("스포너 설정")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoStart = true;

    [Header("생성 방식")]
    [Tooltip("true: 한 개씩만 생성 (파괴 시 다음 생성)\nfalse: 최대 개수까지 생성")]
    [SerializeField] private bool oneAtATime = true;
    [SerializeField] private int maxPlatforms = 5; // oneAtATime이 false일 때 사용

    private GameObject currentPlatform; // 현재 생성된 플랫폼
    private int currentPlatformCount = 0;
    private bool isSpawning = false;
    private bool canSpawn = true;

    void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (oneAtATime)
            {
                // 한 개씩만 생성 모드
                if (currentPlatform == null && canSpawn)
                {
                    SpawnPlatform();
                }
            }
            else
            {
                // 최대 개수까지 생성 모드
                if (currentPlatformCount < maxPlatforms && canSpawn)
                {
                    SpawnPlatform();
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPlatform()
    {
        if (platformPrefab == null)
        {
            Debug.LogError("Platform Prefab이 설정되지 않았습니다!");
            return;
        }

        // 플랫폼 생성
        GameObject platform = Instantiate(platformPrefab, transform.position, Quaternion.identity);

        if (oneAtATime)
        {
            // 한 개씩 모드: 현재 플랫폼 참조 저장
            currentPlatform = platform;
        }
        else
        {
            // 다중 생성 모드: 카운트 증가
            currentPlatformCount++;

            // 파괴 시 카운트 감소 이벤트 등록
            FallingPlatform fallingPlatform = platform.GetComponent<FallingPlatform>();
            if (fallingPlatform != null)
            {
               fallingPlatform.OnDestroyed += OnPlatformDestroyed;
            }
        }

        Debug.Log($"[{gameObject.name}] 플랫폼 생성!");
    }

    void OnPlatformDestroyed()
    {
        currentPlatformCount--;
        Debug.Log($"[{gameObject.name}] 플랫폼 파괴됨. 남은 개수: {currentPlatformCount}");
    }

    // 시간정지 시 호출 (플레이어 레이저에서 호출)
    public void PauseSpawning()
    {
        canSpawn = false;
        Debug.Log($"[{gameObject.name}] 스포너 일시정지");
    }

    // 시간정지 해제 시 호출
    public void ResumeSpawning()
    {
        canSpawn = true;
        Debug.Log($"[{gameObject.name}] 스포너 재개");
    }

    // Gizmo로 스포너 위치 표시
    void OnDrawGizmos()
    {
        Gizmos.color = canSpawn ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 스포너 번호 표시 (Scene 뷰에서)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, gameObject.name);
#endif
    }
}