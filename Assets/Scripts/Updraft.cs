using UnityEngine;
using System.Collections;

public class Updraft : MonoBehaviour
{
    [Header("설정")]
    public bool startActive = true;
    public float windForce = 15f;
    public float maxUpwardSpeed = 8f;

    [Header("연결")]
    public Animator fanAnimator;
    public ParticleSystem windEffect;

    private bool isWorking = false;
    private bool isFrozen = false;
    private SpriteRenderer spriteRenderer;

    public bool IsWorking => isWorking && !isFrozen;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void Freeze(float duration)
    {
        if (isFrozen) return;
        StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        if (windEffect != null) windEffect.Pause();
        if (fanAnimator != null) fanAnimator.speed = 0;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (fanAnimator != null) fanAnimator.speed = 1;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (isFrozen) return;

        if (windEffect != null)
        {
            if (isWorking) windEffect.Play();
            else windEffect.Stop();
        }

        if (fanAnimator != null)
        {
            fanAnimator.SetBool("IsOn", isWorking);

            if (isWorking)
            {
                fanAnimator.SetTrigger("OnTurnOn");
            }
            else
            {
                fanAnimator.SetTrigger("OnTurnOff");
            }
        }
    }
}