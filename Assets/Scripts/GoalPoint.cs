using UnityEngine;
using System.Collections;

public class GoalPoint : MonoBehaviour
{
    [Header("클리어 설정")]
    public string nextSceneName = "Stage2";
    public float clearDelay = 1.0f; // 이펙트를 구경할 시간 (짧게 조절)

    [Header("시각 효과")]
    public GameObject clearEffectPrefab;

    private bool isCleared = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCleared)
        {
            isCleared = true;
            StartCoroutine(ClearRoutine(collision.gameObject));
        }
    }

    IEnumerator ClearRoutine(GameObject player)
    {
        Debug.Log("🎉 스테이지 클리어!");

        if (clearEffectPrefab != null)
        {
            Instantiate(clearEffectPrefab, transform.position, Quaternion.identity);
        }

        if (UIManager.instance != null) UIManager.instance.StopTimer();

        // 플레이어 정지
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 1. 폭죽이나 포즈를 볼 수 있게 잠깐 대기
        yield return new WaitForSeconds(clearDelay);

        // 2. 씬을 직접 넘기지 않고 SceneFader에게 페이드 아웃과 씬 이동을 맡김!
        if (SceneFader.instance != null)
        {
            SceneFader.instance.FadeOutAndLoad(nextSceneName);
        }
        else
        {
            // 혹시 씬에 Fader를 안 만들어뒀다면 예비용으로 그냥 넘어감
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}