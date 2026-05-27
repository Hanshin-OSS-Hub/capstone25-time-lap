using System.Collections;
using UnityEngine;

public class RecallPlatform : MonoBehaviour
{
    [Header("È¸±Í(»ó½Â) ¼³Á¤")]
    public Transform targetPosition;
    public float ascendDuration = 1.0f;
    public float stayDuration = 2.0f;

    [Header("º¹±Í(ÇÏ°­) ¼³Á¤")]
    public float descendDuration = 0.3f;

    private Vector3 originalPosition;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isProcessing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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

        if (animator != null)
        {
            animator.SetTrigger("OnRecall");
        }

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

        if (animator != null)
        {
            animator.SetTrigger("OnBreak");
        }

        isProcessing = false;
    }
}