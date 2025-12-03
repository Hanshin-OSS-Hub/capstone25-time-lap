using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float speed = 5.0f; // 이동 속도
    [SerializeField] private float jumpForce = 8.0f; // 점프 힘

    [Header("점프/바닥 판정")]
    [SerializeField] private LayerMask groundLayer; // 바닥 레이어
    [SerializeField] private int maxJumpCount = 2; // 최대 점프 횟수

    // 상태 변수 (애니메이션 등에서 사용)
    public Animator animator;
    public bool isMove = false;
    public bool isJump = false;
    public bool isDead = false;

    // 내부 로직 변수
    private Rigidbody2D rigid2D;
    private BoxCollider2D boxCollider2D;
    private bool isGrounded;
    private int currentJumpCount = 0;
    private bool isLongJump = false; // 롱 점프 상태

    // 움직이는 플랫폼 관련 변수
    private Vector2 additionalVelocity = Vector2.zero; // 플랫폼 속도
    private Rigidbody2D currentPlatformRB = null; // 현재 밟고 있는 플랫폼

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        // 🟢 [핵심] 벽 끼임 방지 (마찰력 0 설정)
        PhysicsMaterial2D noFrictionMat = new PhysicsMaterial2D("NoFriction");
        noFrictionMat.friction = 0f;
        noFrictionMat.bounciness = 0f;
        boxCollider2D.sharedMaterial = noFrictionMat;
    }

    private void Update()
    {
        if (isDead) return;

        // 1. 이동 입력 처리
        float x = Input.GetAxisRaw("Horizontal");
        Move(x); // 이동 함수 호출

        // 2. 애니메이션 처리
        if (x != 0 && isMove == false)
        {
            isMove = true;
            animator.SetBool("isMove", isMove);
        }
        else if (x == 0 && isMove == true)
        {
            isMove = false;
            animator.SetBool("isMove", isMove);
        }

        // 3. 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // 롱 점프 처리 (스페이스바 유지)
        if (Input.GetKey(KeyCode.Space))
        {
            isLongJump = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isLongJump = false;
        }
    }

    private void FixedUpdate()
    {
        // 1. 바닥 감지
        CheckGround();

        // 2. 점프 횟수 초기화
        if (isGrounded)
        {
            // 땅에 닿으면 점프 횟수를 (최대 - 1)로 초기화 
            // (1번은 땅에서 점프했으므로 공중 점프 횟수만 남김)
            currentJumpCount = maxJumpCount - 1;
        }

        // 3. 점프 높이 조절 (롱 점프 시 중력 감소)
        if (isLongJump && rigid2D.linearVelocity.y > 0)
        {
            rigid2D.gravityScale = 1.0f;
        }
        else
        {
            rigid2D.gravityScale = 2.5f;
        }

        // 4. 움직이는 플랫폼 속도 반영
        if (currentPlatformRB != null)
        {
            additionalVelocity = currentPlatformRB.linearVelocity;
        }
        else
        {
            additionalVelocity = Vector2.zero;
        }
    }

    // 이동 로직
    private void Move(float x)
    {
        // 입력 속도 + 플랫폼 속도를 합산하여 적용
        Vector2 finalVelocity = new Vector2(x * speed, rigid2D.linearVelocity.y);
        rigid2D.linearVelocity = finalVelocity + additionalVelocity;
    }

    // 점프 로직
    private void Jump()
    {
        if (currentJumpCount > 0)
        {
            currentJumpCount--;
            rigid2D.linearVelocity = Vector2.up * jumpForce;
        }
    }

    // 바닥 감지 로직
    private void CheckGround()
    {
        Bounds bounds = boxCollider2D.bounds;
        Vector2 footPosition = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.7f, 0.1f);

        isGrounded = Physics2D.OverlapBox(footPosition, boxSize, 0f, groundLayer);
    }

    // ⭐ 움직이는 플랫폼 감지 (OnCollisionStay2D) ⭐
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 위에서 밟았는지 확인 (Normal 벡터 체크)
            if (collision.contacts[0].normal.y > 0.7f)
            {
                // 플랫폼의 Rigidbody2D를 가져와서 저장
                currentPlatformRB = collision.gameObject.GetComponent<Rigidbody2D>();
            }
        }
    }

    // ⭐ 플랫폼에서 내림 감지 (OnCollisionExit2D) ⭐
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 변수 초기화
            currentPlatformRB = null;
            additionalVelocity = Vector2.zero;
        }
    }

    // 에디터 기즈모 (바닥 감지 범위 표시)
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