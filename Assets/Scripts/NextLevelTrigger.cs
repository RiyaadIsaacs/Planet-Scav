using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("NextLevelTrigger: nextSceneName is not set.", this);
            return;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nextSceneName);
    }
}
