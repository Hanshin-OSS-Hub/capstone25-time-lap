using UnityEngine;
using UnityEngine.Tilemaps;

public class TimeWindow : MonoBehaviour
{
    [Header("설정")]
    public int radius = 2;
    public float duration = 5f;

    [Header("연결")]
    public Tilemap myTilemap;
    private Tilemap sourcePastTilemap;

    void Start()
    {
        // 1. 과거 타일맵 찾기
        GameObject grid = GameObject.Find("Grid");
        if (grid != null)
        {
            Transform pastTr = grid.transform.Find("PastTilemap");
            if (pastTr != null) sourcePastTilemap = pastTr.GetComponent<Tilemap>();
        }

        if (sourcePastTilemap == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3Int cellPos = sourcePastTilemap.WorldToCell(transform.position);
        transform.position = sourcePastTilemap.CellToWorld(cellPos);

        // 2. 타일 복사 실행
        CopyTilesAbsolute();
    }

    void CopyTilesAbsolute()
    {
        Vector3 centerWorldPos = transform.position;
        Vector3Int centerCell = sourcePastTilemap.WorldToCell(centerWorldPos);

        bool hasTile = false;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    Vector3Int targetCellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);

                    if (sourcePastTilemap.HasTile(targetCellPos))
                    {
                        TileBase tile = sourcePastTilemap.GetTile(targetCellPos);

                        // 좌표 변환
                        Vector3 tileWorldPos = sourcePastTilemap.CellToWorld(targetCellPos);
                        Vector3Int myLocalCellPos = myTilemap.WorldToCell(tileWorldPos);

                        // 타일 설치
                        myTilemap.SetTile(myLocalCellPos, tile);

                        // 색상 등 설정
                        myTilemap.SetTileFlags(myLocalCellPos, TileFlags.None);
                        myTilemap.SetColor(myLocalCellPos, new Color(1f, 1f, 1f, 0f));

                        hasTile = true;
                    }
                }
            }
        }

        if (!hasTile)
        {
            Destroy(gameObject);
        }
        else
        {
            myTilemap.RefreshAllTiles();
            Destroy(gameObject, duration);
        }
    }
}