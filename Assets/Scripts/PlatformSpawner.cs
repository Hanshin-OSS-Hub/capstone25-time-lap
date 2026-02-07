using UnityEngine;
using System.Collections;

public class PlatformSpawner : MonoBehaviour
{
    [Header("스포너 설정")]
    public GameObject platformPrefab; // [SerializeField] 대신 public으로 변경 (에디터 접근용)
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoStart = true;

    [Header("생성 방식")]
    [SerializeField] private bool oneAtATime = true;
    [SerializeField] private int maxPlatforms = 5;

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

        if (oneAtATime)
        {
            currentPlatform = platformObj;
        }
        else
        {
            currentPlatformCount++;
        }
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