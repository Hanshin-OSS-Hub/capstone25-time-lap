using UnityEngine;
using System.Collections;

public class RotatingFan : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("총알에 맞았을 때 멈춰있는 시간 (초)")]
    public float freezeDuration = 5f;

    [Header("컴포넌트 연결 (자동)")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private CompositeCollider2D fanCollider;

    private bool isFrozen = false;
    private Coroutine freezeCoroutine;

    void Start()
    {
        fanCollider = GetComponent<CompositeCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 시작 시 애니메이션 재생 (혹시 꺼져있을 경우 대비)
        if (animator != null) animator.speed = 1f;
    }

    // 🟢 [삭제됨] Update에서 transform.Rotate를 하던 코드는 이제 필요 없습니다.
    // 애니메이터가 알아서 그림을 바꿔가며 회전하는 것처럼 보여주기 때문입니다.

    // 외부(총알)에서 호출하는 함수
    public void Freeze(float duration)
    {
        // 이미 얼어있다면 코루틴을 재시작
        if (isFrozen)
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        }

        freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;

        // 1. 캐릭터가 지나갈 수 있게 만들기 (Trigger 설정)
        if (fanCollider != null) fanCollider.isTrigger = true;

        // 2. 시각 효과: 애니메이션 멈춤 & 색상 변경
        // ⭐ 핵심: 애니메이션 속도를 0으로 만들어 멈춥니다. ⭐
        if (animator != null) animator.speed = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f);

        // 3. 대기
        yield return new WaitForSeconds(duration);

        // 4. 원상 복구 (Unfreeze)
        isFrozen = false;

        // 다시 막히는 벽으로 변경
        if (fanCollider != null) fanCollider.isTrigger = false;

        // ⭐ 핵심: 애니메이션 속도를 1로 복구하여 다시 돌게 합니다. ⭐
        if (animator != null) animator.speed = 1f;

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }
}