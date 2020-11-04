using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GameEditor
{

    public class SceneToolMenu
    {
        [MenuItem("SceneTool/跳转到Loading场景")]
        static void GoUIScene()
        {
            EditorSceneManager.OpenScene("Assets/Levels/Loading.unity");
        }


        [MenuItem("SceneTool/跳转到MainCity场景")]
        static void GoMainScene()
        {
            EditorSceneManager.OpenScene("Assets/Levels/MainCity.unity");
        }
        [MenuItem("SceneTool/跳转到Test场景")]
        static void GoTestScene()
        {
            EditorSceneManager.OpenScene("Assets/Levels/Test.unity");
        }
    }
}