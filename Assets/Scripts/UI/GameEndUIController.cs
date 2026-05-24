using UnityEngine;
using UnityEngine.SceneManagement;

/*
역할
1. 게임 종료/클리어 UI 표시
2. 게임 진행 정지
3. Continue / Restart 버튼 처리
*/
public class GameEndUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject endPanel;

    [Header("Options")]
    [SerializeField] private bool pauseGameOnShow = true;
    [SerializeField] private string nextSceneName = "";

    private bool isShown = false;

    public bool IsShown => isShown;

    private void Start()
    {
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    public void ShowClearUI()
    {
        if (isShown)
            return;

        isShown = true;

        if (endPanel != null)
            endPanel.SetActive(true);

        if (pauseGameOnShow)
            Time.timeScale = 0f;

        Sfx.Play(SoundId.ZoneUnlock);
    }

    public void HideUI()
    {
        if (!isShown)
            return;

        isShown = false;

        if (endPanel != null)
            endPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OnClickContinue()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}