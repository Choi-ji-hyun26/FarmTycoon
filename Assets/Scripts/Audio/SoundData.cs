using UnityEngine;

[System.Serializable]
public class SoundData
{
    public SoundId id;

    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Pitch Random Range")]
    public bool useRandomPitch = false;
    [Range(0.5f, 2f)] public float minPitch = 0.95f;
    [Range(0.5f, 2f)] public float maxPitch = 1.05f;

    [Header("Loop")]
    public bool loop = false;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }

    public float GetPitch()
    {
        if (!useRandomPitch)
            return 1f;

        return Random.Range(minPitch, maxPitch);
    }
}