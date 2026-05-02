using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBody : MonoBehaviour
{
    [Header("시간 역행 설정")]
    [Tooltip("몇 초 전의 과거를 기록할 것인가?")]
    public float recordTime = 3f;
    [Tooltip("과거로 끌려가는 연출에 걸리는 시간 (0.3 ~ 0.5초 추천)")]
    public float revertDuration = 0.4f;
    [Tooltip("과거 도착 후 멈춰있는 시간")]
    public float freezeTime = 3f;

    private List<Vector3> positions;
    private Rigidbody2D rb;
    private RigidbodyType2D originalBodyType;

    // 역행 중이거나 정지 중일 때 기록을 멈추기 위한 변수
    private bool isReverting = false;

    void Start()
    {
        positions = new List<Vector3>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            originalBodyType = rb.bodyType;
        }
    }

    void FixedUpdate()
    {
        if (!isReverting)
        {
            Record();
        }
    }

    void Record()
    {
        positions.Insert(0, transform.position);

        int maxFrames = Mathf.RoundToInt(recordTime / Time.fixedDeltaTime);

        if (positions.Count > maxFrames)
        {
            positions.RemoveAt(positions.Count - 1);
        }
    }

    // 총알에 맞았을 때 호출되는 함수
    public void TimeJump()
    {
        if (positions.Count > 0 && !isReverting)
        {
            StartCoroutine(RevertAndFreezeRoutine());
        }
    }

    IEnumerator RevertAndFreezeRoutine()
    {
        isReverting = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = positions[positions.Count - 1]; 
        float elapsedTime = 0f;

        while (elapsedTime < revertDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / revertDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log($"[{gameObject.name}] 과거 위치 도달! {freezeTime}초간 정지합니다.");

        yield return new WaitForSeconds(freezeTime);

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
        }

        positions.Clear();
        isReverting = false;
        Debug.Log($"[{gameObject.name}] 시간 정지 해제! 다시 움직입니다.");
    }
}