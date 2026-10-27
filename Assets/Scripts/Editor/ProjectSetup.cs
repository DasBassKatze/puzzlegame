using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Создаёт сцены «MainMenu» и «Game» и добавляет их в Build Settings.
/// Запускается автоматически при первом открытии проекта, а также из меню Puzzle.
/// </summary>
[InitializeOnLoad]
public static class ProjectSetup
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string MenuScenePath = ScenesFolder + "/MainMenu.unity";
    private const string GameScenePath = ScenesFolder + "/Game.unity";

    static ProjectSetup()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    private static void AutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (File.Exists(MenuScenePath) && File.Exists(GameScenePath))
            return;
        BuildScenes();
    }

    [MenuItem("Puzzle/Пересоздать сцены")]
    public static void BuildScenes()
    {
        Directory.CreateDirectory(ScenesFolder);
        CreateScene(GameScenePath, "GameController", typeof(GameController));
        CreateScene(MenuScenePath, "MenuController", typeof(MainMenuController));

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        PlayerSettings.productName = "Пазл";
        PlayerSettings.companyName = "Shashelov";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.resizableWindow = true;

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(MenuScenePath);
        Debug.Log("Сцены MainMenu и Game созданы и добавлены в Build Settings");
    }

    private static void CreateScene(string path, string controllerName, System.Type controllerType)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = UIFactory.Background;
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0, 0, -10);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        new GameObject(controllerName, controllerType);

        EditorSceneManager.SaveScene(scene, path);
    }
}

/// <summary>
/// Настройки импорта картинок уровней: без сжатия и без масштабирования до степени двойки.
/// </summary>
public class LevelTextureImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Resources/Levels"))
            return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
