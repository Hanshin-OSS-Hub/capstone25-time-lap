using UnityEngine;
using System.Collections;

public class Updraft : MonoBehaviour
{
    [Header("설정")]
    public bool startActive = false;
    public float windForce = 15f;
    public float maxUpwardSpeed = 8f;
    public ParticleSystem windEffect;
    public Animator fanAnimator;

    private bool isWorking = false;
    private bool isFrozen = false; // ❄️ 얼어있는 상태 변수
    private SpriteRenderer spriteRenderer; // 색깔 바꾸기용

    void Start()
    {
        // 부모(본체)에 스프라이트가 있을 수 있으므로 확인
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();

        isWorking = startActive;
        UpdateVisuals();
    }

    public void TurnOn()
    {
        isWorking = true;
        UpdateVisuals();
    }

    public void TurnOff()
    {
        isWorking = false;
        UpdateVisuals();
    }

    // ⭐ [추가됨] 총알이 호출할 얼리기 함수
    public void Freeze(float duration)
    {
        if (isFrozen) return; // 이미 얼었으면 무시
        StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;

        // 1. 시각적 정지 (회색 변환, 파티클/애니메이션 정지)
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        if (windEffect != null) windEffect.Pause();
        if (fanAnimator != null) fanAnimator.speed = 0; // 애니메이션 일시정지

        // 2. 대기
        yield return new WaitForSeconds(duration);

        // 3. 해제
        isFrozen = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (fanAnimator != null) fanAnimator.speed = 1;

        // 원래 상태(켜짐/꺼짐)에 맞춰 복구
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (isFrozen) return; // 얼어있으면 갱신 안 함

        if (windEffect != null)
        {
            if (isWorking) windEffect.Play();
            else windEffect.Stop();
        }
        if (fanAnimator != null) fanAnimator.SetBool("IsOn", isWorking);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // ⭐ 얼어있거나 꺼져있으면 작동 안 함
        if (!isWorking || isFrozen) return;

        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 중력 보정 + 상승력
                float gravityCompensation = rb.mass * Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
                if (rb.linearVelocity.y < maxUpwardSpeed)
                {
                    rb.AddForce(Vector2.up * (gravityCompensation + windForce));
                }
                else
                {
                    rb.AddForce(Vector2.up * gravityCompensation);
                }
            }
        }
    }
}