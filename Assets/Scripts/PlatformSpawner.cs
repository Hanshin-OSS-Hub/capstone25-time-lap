using UnityEngine;
using System.Collections;

public class PlatformSpawner : MonoBehaviour
{
    [Header("스포너 설정")]
    public GameObject platformPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoStart = true;

    [Header("생성 방식")]
    [SerializeField] private bool oneAtATime = true;
    [SerializeField] private int maxPlatforms = 5;

    // 🟢 [추가됨] 스포너 본체의 시간 정지(얼음) 옵션
    [Header("얼음(시간 정지) 옵션")]
    [Tooltip("체크 시 플랫폼이 얼면 다음 플랫폼 생성을 일시정지합니다.")]
    public bool stopWhenFrozen = false;

    // 에디터 스크립트(PlatformSpawnerEditor)가 처리하도록 다시 숨김처리
    [HideInInspector] public FallingPlatformSettings fallingSettings;
    [HideInInspector] public FallingSpikeSettings spikeSettings;

    private GameObject currentPlatform;
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
            // canSpawn이 false(일시정지)면 대기
            if (canSpawn)
            {
                if (oneAtATime)
                {
                    if (currentPlatform == null) SpawnPlatform();
                }
                else
                {
                    if (currentPlatformCount < maxPlatforms) SpawnPlatform();
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPlatform()
    {
        if (platformPrefab == null) return;

        GameObject platformObj = Instantiate(platformPrefab, transform.position, Quaternion.identity);

        FallingPlatform fallingPlatform = platformObj.GetComponentInChildren<FallingPlatform>();
        FallingSpike fallingSpike = platformObj.GetComponentInChildren<FallingSpike>();

        if (fallingPlatform != null)
        {
            fallingPlatform.Init(this, fallingSettings);
            if (!oneAtATime) fallingPlatform.OnDestroyed += OnPlatformDestroyed;
        }
        else if (fallingSpike != null)
        {
            fallingSpike.Init(this, spikeSettings);
            if (!oneAtATime) fallingSpike.OnDestroyed += OnPlatformDestroyed;
        }

        if (oneAtATime) currentPlatform = platformObj;
        else currentPlatformCount++;
    }

    void OnPlatformDestroyed()
    {
        currentPlatformCount--;
    }

    // 🟢 플랫폼이 얼어붙었을 때 호출됨
    public void PauseSpawning()
    {
        // 옵션이 꺼져있으면 멈춤 요청을 무시하고 계속 스폰!
        if (!stopWhenFrozen) return;

        canSpawn = false;
        Debug.Log($"[{gameObject.name}] 스포너 일시정지");
    }

    // 🟢 플랫폼이 녹았을 때 호출됨
    public void ResumeSpawning()
    {
        // 애초에 옵션이 꺼져있었다면 스포너가 멈춘 적도 없으니 무시
        if (!stopWhenFrozen) return;

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