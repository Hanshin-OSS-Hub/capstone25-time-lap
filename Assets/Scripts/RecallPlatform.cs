using System.Collections;
using UnityEngine;

public class RecallPlatform : MonoBehaviour
{
    [Header("회귀(상승) 설정")]
    public Transform targetPosition;    // 떠오를 목표 위치
    public float ascendDuration = 1.0f; // 올라가는 데 걸리는 시간
    public float stayDuration = 2.0f;   // 목표 지점에서 머무는 시간

    [Header("복귀(하강) 설정")]
    public float descendDuration = 0.3f; // 원래 위치로 빠르게 돌아오는 시간

    private Vector3 originalPosition;   // 처음 바닥에 있던 원래 위치를 기억할 변수
    private Rigidbody2D rb;
    private bool isProcessing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        originalPosition = transform.position;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void AscendToTarget()
    {
        if (isProcessing || targetPosition == null) return;

        StartCoroutine(FullRecallRoutine());
    }

    IEnumerator FullRecallRoutine()
    {
        isProcessing = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetPosition.position;
        float elapsedTime = 0f;

        while (elapsedTime < ascendDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / ascendDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        yield return new WaitForSeconds(stayDuration);

        elapsedTime = 0f;
        while (elapsedTime < descendDuration)
        {
            transform.position = Vector3.Lerp(endPos, originalPosition, elapsedTime / descendDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        isProcessing = false;
        Debug.Log($"[{gameObject.name}] 원래 위치로 복귀 완료. 다시 상호작용 가능!");
    }
}