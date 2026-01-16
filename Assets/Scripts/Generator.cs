using UnityEngine;
using UnityEngine.Events;

public class Generator : MonoBehaviour
{
    [Header("초기 상태 설정")]
    public bool startActivated = false; // 초기 상태 설정

    [Header("설정")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactUI; // "E" UI

    [Header("이미지 설정")]
    public Sprite onSprite;  // 켜졌을 때 이미지
    public Sprite offSprite; // 꺼졌을 때 이미지

    [Header("이벤트 연결")]
    public UnityEvent onTurnOn;  // 켜질 때 실행
    public UnityEvent onTurnOff; // 꺼질 때 실행

    // 내부 상태
    private bool isActivated;
    private bool isPlayerInRange = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 1. 초기 상태 적용
        isActivated = startActivated;

        // 2. 초기 상태에 맞춰 이미지 및 이벤트 실행
        UpdateVisuals();
        
        //연결된 장치들의 상태를 동기화하려면 아래 주석 해제
        if (isActivated) onTurnOn.Invoke(); else onTurnOff.Invoke();

        if (interactUI != null) interactUI.SetActive(false);
    }

    void Update()
    {
        // 플레이어가 범위 안에 있고 E키를 누르면 토글
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleGenerator();
        }
    }

    void ToggleGenerator()
    {
        // 상태 전환 (true -> false, false -> true)
        isActivated = !isActivated;

        if (isActivated)
        {
            Debug.Log("발전기 ON");
            onTurnOn.Invoke(); // 켜짐 이벤트 실행
        }
        else
        {
            Debug.Log("발전기 OFF");
            onTurnOff.Invoke(); // 꺼짐 이벤트 실행
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        // 상태에 따라 이미지 교체
        if (isActivated && onSprite != null)
        {
            spriteRenderer.sprite = onSprite;
            spriteRenderer.color = Color.red; // 임시 색 설정
        }
        else if (!isActivated && offSprite != null)
        {
            spriteRenderer.sprite = offSprite;
            spriteRenderer.color = Color.blue; // 임시 색 설정
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