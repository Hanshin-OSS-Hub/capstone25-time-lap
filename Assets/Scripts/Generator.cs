using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Generator : MonoBehaviour
{
    [Header("초기 상태 설정")]
    public bool startActivated = false;

    [Header("설정")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactUI;

    [Header("이미지 설정")]
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("애니메이션 설정")]
    public Animator generatorAnimator;

    [Header("이벤트 연결")]
    public UnityEvent onTurnOn;
    public UnityEvent onTurnOff;

    // 내부 상태
    private bool isActivated;
    private bool isPlayerInRange = false;
    private bool isFrozen = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (generatorAnimator == null) generatorAnimator = GetComponent<Animator>();

        isActivated = startActivated;

        UpdateVisuals();

        if (isActivated) onTurnOn.Invoke(); else onTurnOff.Invoke();

        if (interactUI != null) interactUI.SetActive(false);
    }

    void Update()
    {
        if (isFrozen) return;
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleGenerator();
        }
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
        if (interactUI != null) interactUI.SetActive(false);

        if (generatorAnimator != null) generatorAnimator.speed = 0f;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        if (generatorAnimator != null) generatorAnimator.speed = 1f;

        if (isPlayerInRange && interactUI != null) interactUI.SetActive(true);
    }

    void ToggleGenerator()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            Debug.Log("발전기 ON");
            onTurnOn.Invoke();
        }
        else
        {
            Debug.Log("발전기 OFF");
            onTurnOff.Invoke();
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (generatorAnimator != null)
        {
            generatorAnimator.SetBool("IsOn", isActivated);
        }

        if (spriteRenderer == null) return;

        if (isActivated && onSprite != null)
        {
            spriteRenderer.sprite = onSprite;
        }
        else if (!isActivated && offSprite != null)
        {
            spriteRenderer.sprite = offSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }
}