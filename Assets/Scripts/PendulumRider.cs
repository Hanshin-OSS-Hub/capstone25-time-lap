using UnityEngine;

public class PendulumRider : MonoBehaviour
{
    private Transform originalParent;
    private bool isOnPendulum = false;

    void Start()
    {
        originalParent = transform.parent;
    }

    void LateUpdate()
    {
        if (isOnPendulum)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pendulum"))
        {
            isOnPendulum = true;
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pendulum"))
        {
            isOnPendulum = false;
            transform.SetParent(originalParent);
            transform.rotation = Quaternion.identity;
        }
    }
}