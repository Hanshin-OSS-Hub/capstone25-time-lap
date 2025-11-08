using UnityEngine;

public class Movement2D : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f; //이동 속도
    [SerializeField]
    private float jumpForce = 8.0f; //점프 힘(클수록 높게 점프)
    private Rigidbody2D rigid2D;
    [SerializeField]
    public bool isLongJump = false; // 낮은/높은 점프 체크

    [SerializeField]
    private LayerMask groundLayer; //바닥 체크를 위한 충돌 레이어
    private CapsuleCollider2D capsuleCollider2D; //오브젝트의 충돌 범위 컴포넌트
    private bool isGrounded; //바닥 체크 (바닥에 닿아있을 때 true)
    private Vector3 footPosition; //발의 위치

    [SerializeField]
    private int maxJumpCount = 2; // 땅을 밟기 전까지 할 수 있는 최대 점프 획수
    private int currentJumpCount = 0; // 현재 가능한 점프 횟수


    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        //플레이어 오브젝트의 Collider2D min, center, max 위치 정보
        Bounds bounds = capsuleCollider2D.bounds;
        //플레이어의 발 위치 설정
        footPosition = new Vector2(bounds.center.x, bounds.min.y);
        //플레이어의 발 위치에 원을 생성하고, 원이 바닥과 닿아있으면 isGrounded = true
        isGrounded = Physics2D.OverlapCircle(footPosition, 0.1f, groundLayer);


        // 플레이어의 발이 땅에 닿아 있고, y축 속도가 0이하이면 점프 횟수 초기화 
        // velocity.y <= 0을 추가하지 않으면 점프키를 누르는 순간에도 초기화가 되어 
        // 최대 점프 횟수를 2로 설정하면 3번까지 점프가 가능하게 된다
        if (isGrounded == true && rigid2D.linearVelocity.y <= 0)
        {
            currentJumpCount = maxJumpCount;
        }

        //낮은 점프, 높은 점프 구현 위한 중력 계수 조절 Jump Up일 때만 적용)
        //중력 계수가 낮은 if문은 높은 점프가 되고, 중력 계수가 높은 else문은 낮은 점프가 됨.
        if (isLongJump && rigid2D.linearVelocity.y > 0)
        {
            rigid2D.gravityScale = 1.0f;
        }
        else
        {
            rigid2D.gravityScale = 2.5f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(footPosition, 0.1f);
    }


    public void Move(float x)
    {
        //x축 이동은 x*speed로, y축 이동은 기존의 속력 값(현재는 중력)
        rigid2D.linearVelocity = new Vector2(x * speed, rigid2D.linearVelocity.y);
    }

    public void Jump()
    {
        //if (isGrounded == true)
        if (currentJumpCount > 0)
        {
            //jumpForce의 크기만큼 윗쪽 방향으로 속력 설정
            rigid2D.linearVelocity = Vector2.up * jumpForce;
            // jump 횟수 1 감소
            currentJumpCount--;
        }
    }
}