using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PulsedToneGenerator : MonoBehaviour
{
    [Header("脉冲参数")]
    public float frequency = 450f;       // 正弦波频率
    public float pulseDuration = 1.0f;   // 每个脉冲持续时间（秒）
    public float pulseInterval = 0.5f;   // 脉冲间隔时间（秒）
    public int pulseCount = 4;           // 脉冲数量
    [Range(0f,1f)]
    public float volume = 0.25f;         // 音量

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AudioClip clip = GeneratePulsedTone(frequency, pulseDuration, pulseInterval, pulseCount, volume);
        audioSource.clip = clip;
        audioSource.Play();
    }

    AudioClip GeneratePulsedTone(float freq, float duration, float interval, int count, float vol)
    {
        int sampleRate = 48000;  // Quest 默认采样率
        int pulseSamples = Mathf.RoundToInt(duration * sampleRate);
        int intervalSamples = Mathf.RoundToInt(interval * sampleRate);
        int totalSamples = count * (pulseSamples + intervalSamples);

        float[] data = new float[totalSamples];

        for (int i = 0; i < count; i++)
        {
            int offset = i * (pulseSamples + intervalSamples);
            for (int s = 0; s < pulseSamples; s++)
            {
                data[offset + s] = vol * Mathf.Sin(2 * Mathf.PI * freq * s / sampleRate);
            }
            // 间隔段保持 0 （静音）
        }

        AudioClip clip = AudioClip.Create("PulsedTone", totalSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
