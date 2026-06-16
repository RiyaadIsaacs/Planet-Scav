using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionConfig", menuName = "Planet Scav/Game Session Config")]
public class GameSessionConfig : ScriptableObject
{
    public GameObject playerPrefab;
    public string[] menuSceneNames = { "MainMenu", "Scene Select" };
}
