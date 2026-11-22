using UnityEngine;

/// <summary>
/// The two sounds in this sample are generated in code at startup, not loaded
/// from files. A shot is a noise burst with a fast decay over a short downward
/// sweep; a hit is a low body with a click on its front. Writing them out is
/// a few lines, keeps the repository free of sample packs and licence
/// questions, and makes the envelopes as tunable as the rest of the feel.
/// </summary>
public class ShotAudio : MonoBehaviour
{
	const int SampleRate = 44100;

	public float shotVolume = 0.5f;
	public float hitVolume = 0.6f;

	AudioSource source;
	AudioClip shot;
	AudioClip hit;

	void Awake()
	{
		source = gameObject.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.spatialBlend = 0f;

		shot = BuildShot();
		hit = BuildHit();
	}

	public void PlayShot()
	{
		source.PlayOneShot(shot, shotVolume);
	}

	public void PlayHit()
	{
		source.PlayOneShot(hit, hitVolume);
	}

	AudioClip BuildShot()
	{
		int length = (int)(SampleRate * 0.16f);
		float[] data = new float[length];
		float phase = 0f;

		for (int i = 0; i < length; i++)
		{
			float t = (float)i / SampleRate;
			float decay = Mathf.Exp(-t * 26f);
			float sweep = Mathf.Lerp(220f, 70f, Mathf.Clamp01(t / 0.12f));
			phase += 2f * Mathf.PI * sweep / SampleRate;

			float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 42f);
			float body = Mathf.Sin(phase) * 0.8f;
			data[i] = Mathf.Clamp((noise * 0.7f + body) * decay, -1f, 1f);
		}

		AudioClip clip = AudioClip.Create("Shot", length, 1, SampleRate, false);
		clip.SetData(data, 0);
		return clip;
	}

	AudioClip BuildHit()
	{
		int length = (int)(SampleRate * 0.22f);
		float[] data = new float[length];
		float phase = 0f;

		for (int i = 0; i < length; i++)
		{
			float t = (float)i / SampleRate;
			float decay = Mathf.Exp(-t * 16f);
			float pitch = Mathf.Lerp(190f, 95f, Mathf.Clamp01(t / 0.08f));
			phase += 2f * Mathf.PI * pitch / SampleRate;

			float click = (Random.value * 2f - 1f) * Mathf.Exp(-t * 90f) * 0.5f;
			float body = Mathf.Sin(phase);
			data[i] = Mathf.Clamp((body + click) * decay, -1f, 1f);
		}

		AudioClip clip = AudioClip.Create("Hit", length, 1, SampleRate, false);
		clip.SetData(data, 0);
		return clip;
	}
}
