using UnityEngine;

public class Movement2D : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float speed = 5.0f; // 이동 속도
    [SerializeField] private float jumpForce = 8.0f; // 점프 힘

    [Header("점프/바닥 판정")]
    [SerializeField] private LayerMask groundLayer; // 바닥 레이어
    [SerializeField] private int maxJumpCount = 2; // 최대 점프 횟수

    // 내부 변수
    private Rigidbody2D rigid2D;
    private BoxCollider2D boxCollider2D;
    private bool isGrounded;
    private int currentJumpCount = 0;

    // 외부에서 제어하는 변수
    [HideInInspector] public bool isLongJump = false;
    [HideInInspector] public Vector2 additionalVelocity = Vector2.zero;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // 🟢 [핵심 수정] 벽 끼임 방지 (마찰력 0 설정)
        // 따로 물리 매터리얼을 만들지 않아도, 코드로 즉석에서 '미끄러운 재질'을 만들어 적용합니다.
        PhysicsMaterial2D noFrictionMat = new PhysicsMaterial2D("NoFriction");
        noFrictionMat.friction = 0f;      // 마찰력 제거 (벽에서 미끄러짐)
        noFrictionMat.bounciness = 0f;    // 튕김 제거
        boxCollider2D.sharedMaterial = noFrictionMat;
    }

    private void FixedUpdate()
    {
        // 1. 바닥 감지 (BoxCast로 더 안정적으로 변경)
        Bounds bounds = boxCollider2D.bounds;
        Vector2 footPosition = new Vector2(bounds.center.x, bounds.min.y);

        // 발바닥 너비의 70% 정도만 감지 (가장자리 걸림 방지)
        Vector2 boxSize = new Vector2(bounds.size.x * 0.7f, 0.1f);

        isGrounded = Physics2D.OverlapBox(footPosition, boxSize, 0f, groundLayer);

        // 2. 점프 횟수 초기화 로직
        // 🟢 속도가 0.1f 이하일 때(미세한 떨림 허용) 바닥으로 인정
        if (isGrounded /*&& rigid2D.linearVelocity.y <= 0.1f*/)
        {
            currentJumpCount = maxJumpCount - 1;
        }

        // 3. 점프 높이 조절 (중력 계수 변경)
        if (isLongJump && rigid2D.linearVelocity.y > 0)
        {
            rigid2D.gravityScale = 1.0f;
        }
        else
        {
            rigid2D.gravityScale = 2.5f;
        }
    }

    public void Move(float x)
    {
        // 좌우 이동 (마찰력이 0이어도 velocity를 직접 주므로 미끄러지지 않고 딱 멈춤)
        Vector2 finalVelocity = new Vector2(x * speed, rigid2D.linearVelocity.y);
        rigid2D.linearVelocity = finalVelocity + additionalVelocity;
    }

    public void Jump()
    {
        if (currentJumpCount > 0)
        {
            currentJumpCount--;
            rigid2D.linearVelocity = Vector2.up * jumpForce;
        }
    }

    // 에디터에서 바닥 감지 범위를 보기 위한 함수
    private void OnDrawGizmos()
    {
        if (boxCollider2D != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Bounds bounds = boxCollider2D.bounds;
            Vector2 footPos = new Vector2(bounds.center.x, bounds.min.y);
            Gizmos.DrawWireCube(footPos, new Vector2(bounds.size.x * 0.7f, 0.1f));
        }
    }
}