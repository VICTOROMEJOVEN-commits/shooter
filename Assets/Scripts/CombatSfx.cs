using System.Collections.Generic;
using UnityEngine;

public static class CombatSfx
{
    enum Cue { Shot, PlayerHurt, EnemyHit, EnemyDeath }
    static readonly string[] Names = { "Shot", "PlayerHurt", "EnemyHit", "EnemyDeath" };
    static readonly List<AudioSource> Voices = new List<AudioSource>();
    static AudioClip[] clips = new AudioClip[4];
    static Transform root;
    static int replacement;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        Voices.Clear(); clips = new AudioClip[4]; root = null; replacement = 0;
    }
    public static void Shot(Vector3 position) { Play(Cue.Shot, position, false, 0.62f); }
    public static void PlayerHurt(Vector3 position) { Play(Cue.PlayerHurt, position, false, 0.78f); }
    public static void EnemyHit(Vector3 position) { Play(Cue.EnemyHit, position, true, 0.80f); }
    public static void EnemyDeath(Vector3 position) { Play(Cue.EnemyDeath, position, true, 0.88f); }

    static void Play(Cue cue, Vector3 position, bool spatial, float volume)
    {
        int index = (int)cue;
        if (!clips[index]) clips[index] = Resources.Load<AudioClip>("CombatAudio/" + Names[index]);
        if (!clips[index]) return;
        if (!root) root = new GameObject("Sonidos de combate").transform;
        Voices.RemoveAll(source => !source);
        AudioSource voice = null;
        foreach (var candidate in Voices) if (!candidate.isPlaying) { voice = candidate; break; }
        if (!voice && Voices.Count < 24)
        {
            var obj = new GameObject("Efecto de combate", typeof(AudioSource));
            obj.transform.SetParent(root, false);
            voice = obj.GetComponent<AudioSource>(); Voices.Add(voice);
        }
        if (!voice) voice = Voices[replacement++ % Voices.Count];
        voice.Stop(); voice.transform.position = position;
        voice.name = "SFX - " + Names[index];
        voice.clip = clips[index]; voice.playOnAwake = false; voice.loop = false;
        voice.volume = volume; voice.pitch = Random.Range(0.96f, 1.04f);
        voice.spatialBlend = spatial ? 1 : 0; voice.dopplerLevel = 0;
        voice.rolloffMode = AudioRolloffMode.Logarithmic; voice.minDistance = 2.5f; voice.maxDistance = 32;
        voice.priority = cue == Cue.PlayerHurt ? 48 : cue == Cue.Shot ? 64 : 80;
        voice.Play();
    }
}
