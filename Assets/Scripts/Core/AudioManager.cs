using UnityEngine;

/// <summary>
/// Звуковые эффекты. Звуки синтезируются программно, поэтому проекту не нужны аудиофайлы.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private static bool quitting;

    private AudioSource source;
    private AudioClip clickClip;
    private AudioClip snapClip;
    private AudioClip errorClip;
    private AudioClip winClip;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null && !quitting)
            {
                var go = new GameObject("AudioManager");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AudioManager>();
                instance.Init();
            }
            return instance;
        }
    }

    private void Init()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        clickClip = CreateTone("click", new[] { 880f }, 0.05f);
        snapClip = CreateTone("snap", new[] { 660f, 990f }, 0.07f);
        errorClip = CreateTone("error", new[] { 220f, 170f }, 0.12f);
        winClip = CreateTone("win", new[] { 523f, 659f, 784f, 1047f }, 0.15f);
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    public static void PlayClick() => Instance?.Play(Instance.clickClip);
    public static void PlaySnap() => Instance?.Play(Instance.snapClip);
    public static void PlayError() => Instance?.Play(Instance.errorClip);
    public static void PlayWin() => Instance?.Play(Instance.winClip);

    private void Play(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    /// <summary>Создаёт клип из последовательности затухающих синусоидальных нот.</summary>
    private static AudioClip CreateTone(string name, float[] frequencies, float noteLength)
    {
        const int sampleRate = 44100;
        int samplesPerNote = (int)(sampleRate * noteLength);
        var data = new float[samplesPerNote * frequencies.Length];

        for (int n = 0; n < frequencies.Length; n++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)samplesPerNote;
                envelope *= envelope;
                data[n * samplesPerNote + i] = Mathf.Sin(2f * Mathf.PI * frequencies[n] * t) * 0.35f * envelope;
            }
        }

        AudioClip clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
