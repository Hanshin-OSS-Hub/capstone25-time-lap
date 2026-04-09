using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // 어디서든 쉽게 접근할 수 있도록 싱글톤 패턴 적용
    public static UIManager instance;

    [Header("UI 연결")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bulletText;

    private float playTime = 0f;
    private bool isTimerRunning = true;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 타이머가 켜져 있을 때만 시간 증가
        if (isTimerRunning)
        {
            playTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "Title")
            {
                RestartScene();
            }
        }
    }

    // 시간을 [분:초:밀리초] 형식으로 변환해서 텍스트에 출력
    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        int milliseconds = Mathf.FloorToInt((playTime * 100f) % 100f);

        // 00:00:00 포맷으로 맞춰줌
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    // 외부(총기 스크립트)에서 총알을 쏘거나 장전할 때 부를 함수
    public void UpdateBulletUI(int currentBullets, int maxBullets)
    {
        if (bulletText != null)
        {
            bulletText.text = $"TimeBullet: {currentBullets} / {maxBullets}";
        }
    }

    // 클리어했을 때 타이머를 멈추는 함수
    public void StopTimer()
    {
        isTimerRunning = false;
    }
    public void RestartScene()
    {
        Debug.Log("스테이지 재시작!");

        // 현재 활성화된 씬의 이름을 가져와서 다시 로드합니다.
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}