using UnityEngine;

/// <summary>
/// WASD movement and one action: a short blast that pushes the player back.
/// Movement is deliberately plain. Everything that makes the blast read as a
/// hit is in the layer the switch controls.
///
/// Movement sets horizontal velocity directly, which is predictable to drive
/// but would erase any impulse applied to the body in the same step. The recoil
/// therefore lives in its own velocity term that decays on its own clock and is
/// added on top, so the push survives the movement code instead of being
/// overwritten by it.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerRig : MonoBehaviour
{
	public Transform muzzle;
	public ParticleSystem muzzleBurst;
	public LineRenderer tracer;
	public CameraRig cameraRig;
	public ParticleSystem impactBurst;
	public ShotAudio audioRig;
	public Hitstop hitstop;
	public LayerMask hitMask = ~0;

	public float moveSpeed = 7f;
	public float turnSpeed = 150f;

	/// <summary>Distance the last shot pushed the player, in units.</summary>
	public float LastRecoilUnits { get; private set; }

	Rigidbody body;
	Vector3 knock;
	Vector3 recoilStart;
	bool measuring;
	float cooldownLeft;
	float tracerLeft;

	void Awake()
	{
		body = GetComponent<Rigidbody>();
		body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		if (tracer != null)
		{
			tracer.enabled = false;
		}
	}

	void Update()
	{
		cooldownLeft -= Time.deltaTime;

		if (tracerLeft > 0f)
		{
			tracerLeft -= Time.deltaTime;
			FadeTracer();
		}

		bool firePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
		if (firePressed && cooldownLeft <= 0f)
		{
			Fire();
		}
	}

	void FixedUpdate()
	{
		FeelSettings feel = FeelSettings.Instance;

		float strafe = Input.GetAxisRaw("Horizontal");
		float forward = Input.GetAxisRaw("Vertical");

		Vector3 wish = transform.forward * forward + transform.right * strafe;
		if (wish.sqrMagnitude > 1f)
		{
			wish.Normalize();
		}

		Vector3 planar = wish * moveSpeed;

		if (knock.sqrMagnitude > 0.0001f)
		{
			knock = Vector3.MoveTowards(knock, Vector3.zero, feel.recoilDecay * Time.fixedDeltaTime);
		}
		else if (measuring)
		{
			knock = Vector3.zero;
			LastRecoilUnits = Vector3.ProjectOnPlane(transform.position - recoilStart, Vector3.up).magnitude;
			measuring = false;
		}

		Vector3 velocity = body.linearVelocity;
		body.linearVelocity = new Vector3(planar.x + knock.x, velocity.y, planar.z + knock.z);

		float yaw = 0f;
		if (Input.GetKey(KeyCode.Q)) { yaw -= 1f; }
		if (Input.GetKey(KeyCode.E)) { yaw += 1f; }
		if (Mathf.Abs(yaw) > 0.01f)
		{
			transform.Rotate(Vector3.up, yaw * turnSpeed * Time.fixedDeltaTime);
		}
	}

	void Fire()
	{
		FeelSettings feel = FeelSettings.Instance;
		cooldownLeft = feel.fireCooldown;

		Vector3 from = muzzle.position;
		Vector3 direction = muzzle.forward;
		Vector3 end = from + direction * feel.range;

		RaycastHit hit;
		bool landed = false;
		Vector3 hitPoint = Vector3.zero;
		Vector3 hitNormal = Vector3.up;
		if (Physics.Raycast(from, direction, out hit, feel.range, hitMask, QueryTriggerInteraction.Ignore))
		{
			end = hit.point;
			hitPoint = hit.point;
			hitNormal = hit.normal;
			TargetBlock target = hit.collider.GetComponentInParent<TargetBlock>();
			if (target != null)
			{
				target.TakeHit(direction);
				landed = true;
			}
		}

		ShowTracer(from, end);

		if (!feel.juiceOn)
		{
			// The bare action: the shot happens, nothing announces it.
			LastRecoilUnits = 0f;
			return;
		}

		if (audioRig != null)
		{
			audioRig.PlayShot();
		}

		if (landed)
		{
			if (impactBurst != null)
			{
				impactBurst.transform.position = hitPoint;
				impactBurst.transform.rotation = Quaternion.LookRotation(hitNormal);
				impactBurst.Emit(feel.impactParticles);
			}
			if (audioRig != null)
			{
				audioRig.PlayHit();
			}
			if (hitstop != null)
			{
				hitstop.Freeze(feel.hitstopMs, feel.hitstopScale);
			}
		}

		if (muzzleBurst != null)
		{
			muzzleBurst.Emit(feel.muzzleParticles);
		}

		knock = -direction * feel.recoilSpeed;
		knock.y = 0f;
		recoilStart = transform.position;
		measuring = true;

		if (cameraRig != null)
		{
			cameraRig.Shake(feel.shakeAmplitude, feel.Seconds(feel.shakeMs));
		}
	}

	void ShowTracer(Vector3 from, Vector3 to)
	{
		if (tracer == null)
		{
			return;
		}

		tracer.enabled = true;
		tracer.SetPosition(0, from);
		tracer.SetPosition(1, to);

		FeelSettings feel = FeelSettings.Instance;
		// With the layer off the tracer still shows, so the shot stays readable,
		// but it does not fade: one frame of line, no taper, no afterglow.
		tracerLeft = feel.juiceOn ? feel.Seconds(feel.tracerMs) : Time.deltaTime;
		SetTracerWidth(1f);
	}

	void FadeTracer()
	{
		FeelSettings feel = FeelSettings.Instance;
		if (!feel.juiceOn)
		{
			if (tracerLeft <= 0f)
			{
				tracer.enabled = false;
			}
			return;
		}

		float total = Mathf.Max(0.0001f, feel.Seconds(feel.tracerMs));
		SetTracerWidth(Mathf.Clamp01(tracerLeft / total));
		if (tracerLeft <= 0f)
		{
			tracer.enabled = false;
		}
	}

	void SetTracerWidth(float scale)
	{
		tracer.startWidth = 0.09f * scale;
		tracer.endWidth = 0.02f * scale;
	}
}
