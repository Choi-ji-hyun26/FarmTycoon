using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    [Header("Sound Library")]
    [SerializeField] private SoundData[] sounds;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<SoundId, SoundData> soundMap = new();
    private readonly Queue<AudioSource> availableSources = new();
    private readonly List<AudioSource> allSources = new();

    private const float MinMixerDb = -80f;
    private const float MaxMixerDb = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSoundMap();
        CreateInitialPool();
    }

    private void BuildSoundMap()
    {
        soundMap.Clear();

        if (sounds == null)
            return;

        foreach (var sound in sounds)
        {
            if (sound == null)
                continue;

            if (soundMap.ContainsKey(sound.id))
            {
                Debug.LogWarning($"[AudioManager] Duplicate SoundId found: {sound.id}");
                continue;
            }

            soundMap.Add(sound.id, sound);
        }
    }

    private void CreateInitialPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSource();
        }
    }

    private AudioSource CreateNewSource()
    {
        GameObject go = new GameObject($"SFX_Source_{allSources.Count}");
        go.transform.SetParent(poolRoot != null ? poolRoot : transform);

        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.dopplerLevel = 0f;

        source.spatialBlend = 0f;
        source.minDistance = 1.5f;
        source.maxDistance = 6f;
        source.rolloffMode = AudioRolloffMode.Linear;

        allSources.Add(source);
        availableSources.Enqueue(source);

        return source;
    }

    public void Play(SoundId id)
    {
        if (!TryGetSound(id, out SoundData soundData))
            return;

        AudioClip clip = soundData.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for {id}");
            return;
        }

        AudioSource source = GetAvailableSource();
        ConfigureSource(source, soundData, clip, false, Vector3.zero);
        StartCoroutine(ReturnToPoolWhenFinished(source));
    }

    public void PlayAtPoint(SoundId id, Vector3 worldPosition, float spatialBlend = 1f)
    {
        if (!TryGetSound(id, out SoundData soundData))
            return;

        AudioClip clip = soundData.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for {id}");
            return;
        }

        AudioSource source = GetAvailableSource();
        ConfigureSource(source, soundData, clip, true, worldPosition, spatialBlend);
        StartCoroutine(ReturnToPoolWhenFinished(source));
    }

    public AudioSource PlayLoop(SoundId id, Transform followTarget = null, float spatialBlend = 0f)
    {
        if (!TryGetSound(id, out SoundData soundData))
            return null;

        AudioClip clip = soundData.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for {id}");
            return null;
        }

        AudioSource source = GetAvailableSource();
        ConfigureSource(source, soundData, clip, true, followTarget != null ? followTarget.position : Vector3.zero, spatialBlend);
        source.loop = true;
        source.Play();

        if (followTarget != null)
        {
            source.gameObject.AddComponent<AudioFollowTarget>().Initialize(followTarget);
        }

        return source;
    }

    public void StopLoop(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        ResetSource(source);

        if (!availableSources.Contains(source))
            availableSources.Enqueue(source);
    }

    public void SetSfxVolume(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        float db = normalized <= 0.0001f
            ? MinMixerDb
            : Mathf.Lerp(-20f, MaxMixerDb, normalized);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(sfxVolumeParameter, db);
        }
    }

    public float GetSfxVolumeLinearFromSavedValue(float saved)
    {
        return Mathf.Clamp01(saved);
    }

    private bool TryGetSound(SoundId id, out SoundData soundData)
    {
        if (soundMap.TryGetValue(id, out soundData))
            return true;

        Debug.LogWarning($"[AudioManager] SoundId not registered: {id}");
        return false;
    }

    private AudioSource GetAvailableSource()
    {
        if (availableSources.Count == 0)
        {
            CreateNewSource();
        }

        return availableSources.Dequeue();
    }

    private void ConfigureSource(AudioSource source, SoundData soundData, AudioClip clip, bool usePosition, Vector3 worldPosition, float spatialBlend = 0f)
    {
        source.clip = clip;
        source.volume = soundData.volume;
        source.pitch = soundData.GetPitch();
        source.loop = soundData.loop;
        source.spatialBlend = spatialBlend;

        if (usePosition)
        {
            source.transform.position = worldPosition;

            source.minDistance = 2f;
            source.maxDistance = 12f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        else
        {
            source.transform.localPosition = Vector3.zero;
            source.spatialBlend = 0f;
        }

        source.Play();
    }

    private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source != null && source.isPlaying);

        ResetSource(source);

        if (!availableSources.Contains(source))
            availableSources.Enqueue(source);
    }

    private void ResetSource(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
        source.volume = 1f;
        source.pitch = 1f;
        source.loop = false;
        source.spatialBlend = 0f;

        AudioFollowTarget follow = source.GetComponent<AudioFollowTarget>();
        if (follow != null)
        {
            Destroy(follow);
        }

        source.transform.localPosition = Vector3.zero;
    }
}