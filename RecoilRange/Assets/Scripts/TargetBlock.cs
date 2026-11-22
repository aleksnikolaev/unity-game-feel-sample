using System.Collections;
using UnityEngine;

/// <summary>
/// A block that answers a hit. With the layer on it flashes, punches its own
/// scale and rocks back. With the layer off it only counts the hit, which is
/// the whole argument of this sample in one object.
/// </summary>
public class TargetBlock : MonoBehaviour
{
	public Renderer body;
	public Color restColor = new Color(0.74f, 0.76f, 0.80f);
	public Color hitColor = new Color(1f, 0.92f, 0.55f);
	public float knockback = 1.6f;

	Material material;
	Vector3 restScale;
	Rigidbody physics;
	Coroutine flashing;
	Coroutine punching;
	int hits;

	public int Hits { get { return hits; } }

	void Awake()
	{
		if (body == null)
		{
			body = GetComponentInChildren<Renderer>();
		}
		material = body.material;
		restScale = transform.localScale;
		physics = GetComponent<Rigidbody>();
		Paint(restColor, 0f);
	}

	public void TakeHit(Vector3 direction)
	{
		hits++;

		FeelSettings feel = FeelSettings.Instance;
		if (!feel.juiceOn)
		{
			return;
		}

		if (flashing != null) { StopCoroutine(flashing); }
		flashing = StartCoroutine(Flash(feel.Seconds(feel.flashMs)));

		if (punching != null) { StopCoroutine(punching); }
		punching = StartCoroutine(Punch(feel.hitPunch, feel.Seconds(feel.punchMs)));

		if (physics != null)
		{
			physics.AddForce(direction.normalized * knockback, ForceMode.Impulse);
		}
	}

	IEnumerator Flash(float seconds)
	{
		Paint(hitColor, 1f);
		yield return MicroTween.Run(seconds, t =>
		{
			Paint(Color.Lerp(hitColor, restColor, t), 1f - t);
		}, MicroTween.OutCubic);
		Paint(restColor, 0f);
		flashing = null;
	}

	IEnumerator Punch(float peak, float seconds)
	{
		yield return MicroTween.Run(seconds, t =>
		{
			float amount = Mathf.Lerp(1f, peak, MicroTween.Punch(t));
			transform.localScale = restScale * amount;
		});
		transform.localScale = restScale;
		punching = null;
	}

	void Paint(Color color, float glow)
	{
		material.color = color;
		material.EnableKeyword("_EMISSION");
		material.SetColor("_EmissionColor", hitColor * glow * 1.8f);
	}
}
