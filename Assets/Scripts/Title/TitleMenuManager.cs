using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [Header("씬 설정")]
    [Tooltip("시작 버튼을 누르면 이동할 게임 씬의 이름")]
    public string firstLevelName = "Stage1";

    public void StartGame()
    {
        Debug.Log("새 게임 시작!");
        SceneManager.LoadScene("Bin"); 
    }

    public void ContinueGame()
    {
        Debug.Log("이어하기 (세이브 데이터 로드 필요)");
    }

    public void OpenSettings()
    {
        Debug.Log("설정 창 열기");
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료!");

        Application.Quit();
    }
}