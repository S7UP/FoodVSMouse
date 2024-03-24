using UnityEditor;

using UnityEngine;
public class StageEditor
{
    [MenuItem("StageEditor/AddNewEnemy")]
    static void AddNewEnemy()
    {
        Application.OpenURL(Application.persistentDataPath);
    }
}
