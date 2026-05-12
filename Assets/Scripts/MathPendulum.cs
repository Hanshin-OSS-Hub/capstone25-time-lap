using System.Collections;
using UnityEngine;

public class MathPendulum : MonoBehaviour
{
    [Header("가짜 진자 설정")]
    public Transform anchor;      // 천장(기준점) 오브젝트를 연결할 칸
    public float maxAngle = 60f;  // 최대 회전 각도 (얼마나 높이 올라갈지)
    public float speed = 2f;      // 흔들리는 속도

    [Header("시간 정지 설정")]
    public float freezeDuration = 3f; // 얼어있는 시간

    private Rigidbody2D rb;
    private float timer = 0f;
    private float length;
    private bool isFrozen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (anchor != null)
        {
            length = Vector2.Distance(anchor.position, transform.position);
        }
    }

    void FixedUpdate()
    {
        if (isFrozen || anchor == null) return;
        timer += Time.fixedDeltaTime;
        float currentAngle = maxAngle * Mathf.Sin(timer * speed);
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 newPos = new Vector2(
            anchor.position.x + length * Mathf.Sin(angleRad),
            anchor.position.y - length * Mathf.Cos(angleRad)
        );

        rb.MovePosition(newPos);
        rb.MoveRotation(currentAngle);
    }

    public void FreezeTime()
    {
        if (isFrozen) return;
        StartCoroutine(FreezeRoutine());
    }

    IEnumerator FreezeRoutine()
    {
        isFrozen = true;
        Debug.Log($"[{gameObject.name}] 가짜 진자 시간 정지!");

        yield return new WaitForSeconds(freezeDuration);

        isFrozen = false;
        Debug.Log($"[{gameObject.name}] 진자 정지 해제! 자연스럽게 이어서 스윙합니다.");
    }
}