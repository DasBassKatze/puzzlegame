using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Переходы между сценами с плавным затемнением экрана.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public const string MenuScene = "MainMenu";
    public const string GameScene = "Game";

    private const float FadeDuration = 0.35f;

    private static SceneLoader instance;

    private CanvasGroup group;
    private bool busy;

    private static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                Canvas canvas = UIFactory.CreateCanvas("SceneFader", 1000);
                DontDestroyOnLoad(canvas.gameObject);
                var fade = UIFactory.CreateImage("Fade", canvas.transform, Color.black);
                UIFactory.Stretch(fade.rectTransform);
                instance = canvas.gameObject.AddComponent<SceneLoader>();
                instance.group = canvas.gameObject.AddComponent<CanvasGroup>();
                instance.group.alpha = 0f;
                instance.group.blocksRaycasts = false;
            }
            return instance;
        }
    }

    public static void Load(string sceneName)
    {
        Instance.StartLoading(sceneName);
    }

    private void StartLoading(string sceneName)
    {
        if (busy)
            return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        busy = true;
        group.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        Time.timeScale = 1f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
            yield return null;

        yield return Fade(1f, 0f);
        group.blocksRaycasts = false;
        busy = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < FadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / FadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}
