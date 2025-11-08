using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Movement2D movement2D;

    private void Awake()
    {
        movement2D = GetComponent<Movement2D>();
    }
    private void Update()
    {
        //플레이어 이동
        // left or a = -1 / right or d = 1
        float x = Input.GetAxisRaw("Horizontal");
        // 좌우 이동 방향 제어
        movement2D.Move(x);

        //플레이어 점프 (스페이스 바를 누르면 점프)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movement2D.Jump();
        }


        // 스페이스 키를 누르고 있으면 isLongJump = true
        if (Input.GetKey(KeyCode.Space))
        {
            movement2D.isLongJump = true;
        }
        // 스페이스 키를 떼면 isLongJump = false
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            movement2D.isLongJump = false;
        }

    }
}