using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    private AudioSource _source;

    // Cached clips
    private AudioClip _shootClip;
    private AudioClip _hitClip;
    private AudioClip _deathClip;
    private AudioClip _levelUpClip;
    private AudioClip _buyClip;
    private AudioClip _bossClip;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.volume = 0.3f;

        GenerateClips();
    }

    private void GenerateClips()
    {
        _shootClip = GenerateTone(800f, 0.04f, 0.15f);    // short high beep
        _hitClip = GenerateTone(300f, 0.05f, 0.2f);       // low thud
        _deathClip = GenerateSweep(600f, 150f, 0.12f, 0.25f); // descending
        _levelUpClip = GenerateSweep(400f, 900f, 0.15f, 0.3f); // ascending
        _buyClip = GenerateTone(1200f, 0.08f, 0.2f);      // ding
        _bossClip = GenerateTone(80f, 0.3f, 0.5f);        // low rumble
    }

    public static void PlayShoot() => Instance?._source.PlayOneShot(Instance._shootClip);
    public static void PlayHit() => Instance?._source.PlayOneShot(Instance._hitClip);
    public static void PlayDeath() => Instance?._source.PlayOneShot(Instance._deathClip);
    public static void PlayLevelUp() => Instance?._source.PlayOneShot(Instance._levelUpClip);
    public static void PlayBuy() => Instance?._source.PlayOneShot(Instance._buyClip);
    public static void PlayBoss() => Instance?._source.PlayOneShot(Instance._bossClip);

    private AudioClip GenerateTone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration); // linear fade out
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateSweep(float startFreq, float endFreq, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(startFreq, endFreq, progress);
            float envelope = 1f - progress;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("sweep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
