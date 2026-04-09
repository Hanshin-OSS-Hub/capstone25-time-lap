using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    // 어디서든 쉽게 부를 수 있게 싱글톤 패턴 사용
    public static SceneFader instance;

    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1.0f; // 까매지거나 밝아지는데 걸리는 시간

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 씬이 시작되면 무조건 화면을 가리고 시작해서 부드럽게 밝아짐 (페이드 인)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeRoutine(1f, 0f, null)); // 알파값 1(검정) -> 0(투명)
        }
    }

    // 외부(GoalPoint 등)에서 호출할 페이드 아웃 함수
    public void FadeOutAndLoad(string nextSceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeRoutine(0f, 1f, nextSceneName)); // 알파값 0(투명) -> 1(검정)
        }
        else
        {
            // 이미지가 없으면 그냥 바로 넘어감
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeRoutine(float startAlpha, float targetAlpha, string nextScene)
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // 정확한 목표값으로 맞추기
        color.a = targetAlpha;
        fadeImage.color = color;

        // 페이드 아웃이 끝났고, 넘어갈 씬 이름이 있다면 씬 이동
        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
        else if (targetAlpha == 0f)
        {
            // 페이드 인이 끝나서 투명해졌다면 이미지를 꺼서 성능 최적화
            fadeImage.gameObject.SetActive(false);
        }
    }
}