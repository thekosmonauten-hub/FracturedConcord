using UnityEngine;

[CreateAssetMenu(fileName = "MusicTrack", menuName = "Dexiled/Audio/Music Track")]
public class MusicTrack : ScriptableObject
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
}
