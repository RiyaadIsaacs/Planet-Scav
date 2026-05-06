using UnityEngine;

public class NextLevelTrigger : MonoBehaviour
{
    [SerializeField] public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
