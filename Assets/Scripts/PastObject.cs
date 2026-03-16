using UnityEngine;
using System.Collections;

public class PastObject : MonoBehaviour
{
    [Header("과거 모습 오브젝트")]
    [Tooltip("총알에 맞으면 나타날 실제 과거 오브젝트 (자식 오브젝트를 넣으세요)")]
    public GameObject pastContent;

    private Coroutine revertCoroutine;

    void Start()
    {
        // 게임 시작 시 과거의 모습은 보이지 않게 꺼둡니다.
        if (pastContent != null)
        {
            pastContent.SetActive(false);
        }
    }

    // 총알 폭발 범위에 닿았을 때 호출됨
    public void ActivatePast(float duration)
    {
        // 이미 켜져 있는 상태에서 또 맞으면 시간 연장
        if (revertCoroutine != null)
        {
            StopCoroutine(revertCoroutine);
        }

        revertCoroutine = StartCoroutine(RevertRoutine(duration));
    }

    IEnumerator RevertRoutine(float duration)
    {
        // 1. 과거 오브젝트 등장 (발판 생성)
        if (pastContent != null)
        {
            pastContent.SetActive(true);
        }

        // 2. 유지 시간 대기
        yield return new WaitForSeconds(duration);

        // 3. 다시 사라짐 (발판 소멸)
        if (pastContent != null)
        {
            pastContent.SetActive(false);
        }

        revertCoroutine = null;
    }
}