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
    [SerializeField] private int maxPlatforms = 5;

    private GameObject currentPlatform;
    private int currentPlatformCount = 0;
    private bool isSpawning = false;
    private bool canSpawn = true; // 일시정지 제어 플래그

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
            // canSpawn이 false(일시정지)면 대기
            if (canSpawn)
            {
                if (oneAtATime)
                {
                    if (currentPlatform == null)
                    {
                        SpawnPlatform();
                    }
                }
                else
                {
                    if (currentPlatformCount < maxPlatforms)
                    {
                        SpawnPlatform();
                    }
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPlatform()
    {
        if (platformPrefab == null) return;

        GameObject platformObj = Instantiate(platformPrefab, transform.position, Quaternion.identity);

        // 🟢 [추가] 생성된 플랫폼에게 스포너 정보 전달
        FallingPlatform fallingPlatform = platformObj.GetComponent<FallingPlatform>();
        if (fallingPlatform != null)
        {
            fallingPlatform.Init(this);

            if (!oneAtATime)
            {
                // 다중 생성 모드일 때 파괴 이벤트 연결
                fallingPlatform.OnDestroyed += OnPlatformDestroyed;
            }
        }

        if (oneAtATime)
        {
            currentPlatform = platformObj;
        }
        else
        {
            currentPlatformCount++;
        }

        Debug.Log($"[{gameObject.name}] 플랫폼 생성!");
    }

    void OnPlatformDestroyed()
    {
        currentPlatformCount--;
    }

    public void PauseSpawning()
    {
        canSpawn = false;
        Debug.Log($"[{gameObject.name}] 스포너 일시정지");
    }

    public void ResumeSpawning()
    {
        canSpawn = true;
        Debug.Log($"[{gameObject.name}] 스포너 재개");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = canSpawn ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, gameObject.name);
#endif
    }
}