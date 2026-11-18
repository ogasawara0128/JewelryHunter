using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip meGameClear;
    public AudioClip meGameOver;

    // 💡 インスペクターから直接 AudioSource をアタッチできるようにする
    [SerializeField] private AudioSource soundPlayer;

    public void ClearSound()
    {
        if (soundPlayer == null) return;
        soundPlayer.Stop();
        soundPlayer.PlayOneShot(meGameClear);
    }

    public void OverSound()
    {
        if (soundPlayer == null) return;
        soundPlayer.Stop();
        soundPlayer.PlayOneShot(meGameOver);
    }
}