using UnityEngine;

/// <summary>
/// Every number that makes the shot feel like something lives here, in one place,
/// so it can be tuned in the inspector while the game runs. The master switch
/// turns all of it off at once, which is the point of this sample: the same
/// action, with and without the layer on top.
/// </summary>
public class FeelSettings : MonoBehaviour
{
	public static FeelSettings Instance { get; private set; }

	[Header("Master switch")]
	public bool juiceOn = true;

	[Header("Recoil")]
	[Tooltip("Speed the shot pushes the player back with, in units per second.")]
	public float recoilSpeed = 5.4f;
	[Tooltip("How fast that push dies out, in units per second squared.")]
	public float recoilDecay = 18f;

	[Header("Camera shake")]
	[Tooltip("Peak camera offset, in units.")]
	public float shakeAmplitude = 0.25f;
	[Tooltip("Shake length, in milliseconds.")]
	public int shakeMs = 180;

	[Header("Target flash")]
	[Tooltip("How long the target stays lit, in milliseconds.")]
	public int flashMs = 120;
	[Tooltip("Scale punch on hit, 1 means no punch.")]
	public float hitPunch = 1.18f;
	[Tooltip("Length of the scale punch, in milliseconds.")]
	public int punchMs = 220;

	[Header("Muzzle")]
	[Tooltip("Particles released per shot.")]
	public int muzzleParticles = 24;
	[Tooltip("Tracer fade, in milliseconds.")]
	public int tracerMs = 90;

	[Header("Fire")]
	public float fireCooldown = 0.35f;
	public float range = 45f;

	void Awake()
	{
		Instance = this;
	}

	public float Seconds(int milliseconds)
	{
		return milliseconds / 1000f;
	}
}
