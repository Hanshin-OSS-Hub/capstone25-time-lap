using UnityEngine;

public class WindArea : MonoBehaviour
{
    private Updraft parentUpdraft;

    void Start()
    {
        parentUpdraft = GetComponentInParent<Updraft>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 1. 부모가 없거나, 부모가 작동 중이 아니면 바람 안 붊
        if (parentUpdraft == null || !parentUpdraft.IsWorking) return;

        // 2. 플레이어 띄우기 로직
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 부모에 설정된 바람 세기 값을 가져옴
                float gravityCompensation = rb.mass * Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;

                if (rb.linearVelocity.y < parentUpdraft.maxUpwardSpeed)
                {
                    rb.AddForce(Vector2.up * (gravityCompensation + parentUpdraft.windForce));
                }
                else
                {
                    rb.AddForce(Vector2.up * gravityCompensation);
                }
            }
        }
    }
}