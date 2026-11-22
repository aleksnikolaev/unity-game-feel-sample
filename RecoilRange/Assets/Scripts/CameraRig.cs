using System.Collections;
using UnityEngine;

/// <summary>
/// Follows the player and shakes on command. The shake decays over its own
/// duration, so a longer shake is not a louder shake.
/// </summary>
public class CameraRig : MonoBehaviour
{
	public Transform follow;
	public Vector3 offset = new Vector3(0f, 4.2f, -7.5f);
	public float followLag = 0.12f;

	Vector3 velocity;
	Vector3 shakeOffset;
	Coroutine shaking;

	void LateUpdate()
	{
		if (follow == null)
		{
			return;
		}

		Vector3 wanted = follow.position + follow.TransformVector(offset);
		Vector3 smoothed = Vector3.SmoothDamp(transform.position - shakeOffset, wanted, ref velocity, followLag);
		transform.position = smoothed + shakeOffset;
		transform.LookAt(follow.position + Vector3.up * 1.1f + shakeOffset);
	}

	public void Shake(float amplitude, float seconds)
	{
		if (shaking != null)
		{
			StopCoroutine(shaking);
		}
		shaking = StartCoroutine(ShakeRoutine(amplitude, seconds));
	}

	IEnumerator ShakeRoutine(float amplitude, float seconds)
	{
		yield return MicroTween.Run(seconds, t =>
		{
			float decay = 1f - t;
			shakeOffset = new Vector3(
				(Mathf.PerlinNoise(Time.time * 36f, 0f) - 0.5f) * 2f,
				(Mathf.PerlinNoise(0f, Time.time * 36f) - 0.5f) * 2f,
				0f) * amplitude * decay;
		});

		shakeOffset = Vector3.zero;
		shaking = null;
	}
}
