using UnityEngine;

public class UIManager1 : MonoBehaviour
{
    public GameObject gamePanel;

    void Update()
    {
        if (gamePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            gamePanel.SetActive(false);
        }
    }

    public void OpenGamePanel()
    {
        gamePanel.SetActive(true);
    }
}
