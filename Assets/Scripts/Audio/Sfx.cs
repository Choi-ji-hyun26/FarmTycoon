using UnityEngine;

public static class Sfx
{
    public static void Play(SoundId id)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.Play(id);
    }

    // 공간 감 제공
    public static void PlayAtPoint(SoundId id, Vector3 position, float spatialBlend = 1f)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlayAtPoint(id, position, spatialBlend);
    }
}