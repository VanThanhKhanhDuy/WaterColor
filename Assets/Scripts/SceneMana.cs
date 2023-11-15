using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMana : MonoBehaviour
{
    public static class SceneNames
    {
        public const string LV1 = "LV1";
        public const string LV2 = "LV2";
        public const string LV3 = "LV3";
        public const string LV4 = "LV4";
        public const string VictoryScene = "VictoryScene";
        public const string Menu = "Menu";
    }

    public static void SceneManagement()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        switch (currentSceneName)
        {
            case SceneNames.LV1:
                SceneManager.LoadScene(SceneNames.LV2);
                break;

            case SceneNames.LV2:
                SceneManager.LoadScene(SceneNames.LV3);
                break;

            case SceneNames.LV3:
                SceneManager.LoadScene(SceneNames.LV4);
                break;

            case SceneNames.LV4:
                SceneManager.LoadScene(SceneNames.VictoryScene);
                break;
            default:
                break;
        }
    }

    
    public static void GoToLevel1()
    {
        SceneManager.LoadScene(SceneNames.LV1);
    }
    public static void GoToMenu()
    {
        SceneManager.LoadScene(SceneNames.Menu);
    }
}