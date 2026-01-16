using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 부딪힌 물체에서 PlayerController를 찾음
            PlayerController player = collision.GetComponent<PlayerController>();

            // 2. 플레이어가 맞다면 Die() 함수 호출
            if (player != null)
            {
                player.Die();
            }
        }
    }
}