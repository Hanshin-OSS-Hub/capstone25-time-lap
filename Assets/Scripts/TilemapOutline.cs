using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections; // 코루틴 사용을 위해 필수

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapOutline : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private CompositeCollider2D compositeCollider;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        compositeCollider = GetComponent<CompositeCollider2D>();
    }

    public void DrawOutline()
    {
        // 즉시 실행하지 않고 코루틴 시작
        StartCoroutine(DrawOutlineRoutine());
    }

    IEnumerator DrawOutlineRoutine()
    {
        // 물리 엔진이 콜라이더를 합칠 시간을 한 프레임 줍니다.
        yield return new WaitForFixedUpdate();

        // 혹시 모르니 강제 생성 한 번 더
        compositeCollider.GenerateGeometry();

        if (compositeCollider.pathCount > 0)
        {
            // 경로 점 가져오기
            Vector2[] points = new Vector2[compositeCollider.GetPathPointCount(0)];
            compositeCollider.GetPath(0, points);

            // 라인 렌더러 점 개수 설정
            lineRenderer.positionCount = points.Length;

            // 점 연결
            for (int i = 0; i < points.Length; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}