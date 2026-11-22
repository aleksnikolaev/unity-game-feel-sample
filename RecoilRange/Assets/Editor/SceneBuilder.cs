using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// The scene is built by this script rather than by hand, so anyone can read
/// what is in it and rebuild it from scratch with one command.
/// </summary>
public static class SceneBuilder
{
	const string ScenePath = "Assets/Scenes/Range.unity";

	[MenuItem("Range/Rebuild scene")]
	public static void Build()
	{
		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		Directory.CreateDirectory("Assets/Scenes");
		Directory.CreateDirectory("Assets/Materials");

		Material groundMat = MakeMaterial("Ground", new Color(0.30f, 0.32f, 0.35f));
		Material playerMat = MakeMaterial("Player", new Color(0.26f, 0.55f, 0.72f));
		Material targetMat = MakeMaterial("Target", new Color(0.74f, 0.76f, 0.80f));

		GameObject settingsHolder = new GameObject("FeelSettings");
		FeelSettings feel = settingsHolder.AddComponent<FeelSettings>();

		GameObject sun = new GameObject("Sun");
		Light light = sun.AddComponent<Light>();
		light.type = LightType.Directional;
		light.intensity = 1.05f;
		light.shadows = LightShadows.Soft;
		sun.transform.rotation = Quaternion.Euler(48f, 32f, 0f);

		GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
		ground.name = "Ground";
		ground.transform.localScale = new Vector3(8f, 1f, 8f);
		ground.GetComponent<Renderer>().sharedMaterial = groundMat;

		GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		player.name = "Player";
		player.transform.position = new Vector3(0f, 1.1f, -8f);
		player.GetComponent<Renderer>().sharedMaterial = playerMat;
		Rigidbody playerBody = player.AddComponent<Rigidbody>();
		playerBody.mass = 1.4f;
		playerBody.linearDamping = 2.2f;
		playerBody.interpolation = RigidbodyInterpolation.Interpolate;

		GameObject muzzle = new GameObject("Muzzle");
		muzzle.transform.SetParent(player.transform, false);
		muzzle.transform.localPosition = new Vector3(0f, 0.35f, 0.75f);

		ParticleSystem burst = MakeMuzzle(muzzle.transform);
		LineRenderer tracer = MakeTracer(player.transform);

		GameObject cameraObject = new GameObject("Main Camera");
		cameraObject.tag = "MainCamera";
		Camera camera = cameraObject.AddComponent<Camera>();
		camera.clearFlags = CameraClearFlags.Skybox;
		camera.fieldOfView = 62f;
		cameraObject.AddComponent<AudioListener>();
		CameraRig rig = cameraObject.AddComponent<CameraRig>();
		rig.follow = player.transform;
		cameraObject.transform.position = new Vector3(0f, 5f, -16f);

		PlayerRig playerRig = player.AddComponent<PlayerRig>();
		playerRig.muzzle = muzzle.transform;
		playerRig.muzzleBurst = burst;
		playerRig.tracer = tracer;
		playerRig.cameraRig = rig;

		Vector3[] spots = {
			new Vector3(-4.5f, 1f, 9f),
			new Vector3(0f, 1f, 14f),
			new Vector3(5f, 1f, 10.5f)
		};
		for (int i = 0; i < spots.Length; i++)
		{
			MakeTarget(spots[i], targetMat, i + 1);
		}

		BuildUI();

		EditorSceneManager.SaveScene(scene, ScenePath);
		EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
		AssetDatabase.SaveAssets();
		Debug.Log("RANGE SCENE BUILT " + ScenePath + " targets " + spots.Length + " feel " + feel.juiceOn);
	}

	static Material MakeMaterial(string name, Color color)
	{
		string path = "Assets/Materials/" + name + ".mat";
		Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (existing != null)
		{
			existing.color = color;
			return existing;
		}

		Material material = new Material(Shader.Find("Standard"));
		material.color = color;
		material.SetFloat("_Glossiness", 0.25f);
		AssetDatabase.CreateAsset(material, path);
		return material;
	}

	static ParticleSystem MakeMuzzle(Transform parent)
	{
		GameObject holder = new GameObject("MuzzleBurst");
		holder.transform.SetParent(parent, false);
		ParticleSystem particles = holder.AddComponent<ParticleSystem>();

		var main = particles.main;
		main.duration = 1f;
		main.loop = false;
		main.playOnAwake = false;
		main.startLifetime = 0.35f;
		main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 11f);
		main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
		main.startColor = new ParticleSystem.MinMaxGradient(
			new Color(1f, 0.86f, 0.45f), new Color(1f, 0.56f, 0.22f));
		main.gravityModifier = 0.35f;
		main.simulationSpace = ParticleSystemSimulationSpace.World;

		var emission = particles.emission;
		emission.enabled = false;

		var shape = particles.shape;
		shape.shapeType = ParticleSystemShapeType.Cone;
		shape.angle = 16f;
		shape.radius = 0.05f;

		var sizeOverLife = particles.sizeOverLifetime;
		sizeOverLife.enabled = true;
		sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

		ParticleSystemRenderer renderer = holder.GetComponent<ParticleSystemRenderer>();
		renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
		renderer.material.SetColor("_Color", Color.white);

		return particles;
	}

	static LineRenderer MakeTracer(Transform parent)
	{
		GameObject holder = new GameObject("Tracer");
		holder.transform.SetParent(parent, false);
		LineRenderer line = holder.AddComponent<LineRenderer>();
		line.useWorldSpace = true;
		line.positionCount = 2;
		line.numCapVertices = 2;
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.startColor = new Color(1f, 0.93f, 0.62f);
		line.endColor = new Color(1f, 0.62f, 0.28f, 0.35f);
		line.enabled = false;
		return line;
	}

	static void MakeTarget(Vector3 position, Material material, int index)
	{
		GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
		target.name = "Target " + index;
		target.transform.position = position;
		target.transform.localScale = new Vector3(1.4f, 2f, 1.4f);
		target.GetComponent<Renderer>().sharedMaterial = material;

		Rigidbody physics = target.AddComponent<Rigidbody>();
		physics.mass = 2.6f;
		physics.linearDamping = 1.1f;
		physics.angularDamping = 2f;

		target.AddComponent<TargetBlock>();
	}

	static void BuildUI()
	{
		GameObject canvasObject = new GameObject("UI");
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1280f, 720f);
		canvasObject.AddComponent<GraphicRaycaster>();

		new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

		Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		GameObject buttonObject = new GameObject("FeelToggle", typeof(Image), typeof(Button));
		buttonObject.transform.SetParent(canvasObject.transform, false);
		RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
		buttonRect.anchorMin = new Vector2(1f, 1f);
		buttonRect.anchorMax = new Vector2(1f, 1f);
		buttonRect.pivot = new Vector2(1f, 1f);
		buttonRect.anchoredPosition = new Vector2(-24f, -24f);
		buttonRect.sizeDelta = new Vector2(260f, 56f);
		buttonObject.GetComponent<Image>().color = new Color(0.17f, 0.45f, 0.30f);

		GameObject labelObject = new GameObject("Label", typeof(Text));
		labelObject.transform.SetParent(buttonObject.transform, false);
		Text label = labelObject.GetComponent<Text>();
		label.font = font;
		label.fontSize = 20;
		label.alignment = TextAnchor.MiddleCenter;
		label.color = Color.white;
		RectTransform labelRect = labelObject.GetComponent<RectTransform>();
		labelRect.anchorMin = Vector2.zero;
		labelRect.anchorMax = Vector2.one;
		labelRect.offsetMin = Vector2.zero;
		labelRect.offsetMax = Vector2.zero;

		GameObject helpObject = new GameObject("Help", typeof(Text));
		helpObject.transform.SetParent(canvasObject.transform, false);
		Text help = helpObject.GetComponent<Text>();
		help.font = font;
		help.fontSize = 18;
		help.alignment = TextAnchor.LowerLeft;
		help.color = Color.white;
		Outline helpOutline = helpObject.AddComponent<Outline>();
		helpOutline.effectColor = new Color(0f, 0f, 0f, 0.75f);
		helpOutline.effectDistance = new Vector2(1.3f, -1.3f);
		RectTransform helpRect = helpObject.GetComponent<RectTransform>();
		helpRect.anchorMin = new Vector2(0f, 0f);
		helpRect.anchorMax = new Vector2(0f, 0f);
		helpRect.pivot = new Vector2(0f, 0f);
		helpRect.anchoredPosition = new Vector2(24f, 20f);
		helpRect.sizeDelta = new Vector2(760f, 40f);

		GameObject numbersPanel = new GameObject("NumbersPanel", typeof(Image));
		numbersPanel.transform.SetParent(canvasObject.transform, false);
		numbersPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
		RectTransform panelRect = numbersPanel.GetComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(1f, 1f);
		panelRect.anchorMax = new Vector2(1f, 1f);
		panelRect.pivot = new Vector2(1f, 1f);
		panelRect.anchoredPosition = new Vector2(-24f, -88f);
		panelRect.sizeDelta = new Vector2(640f, 34f);

		GameObject numbersObject = new GameObject("Numbers", typeof(Text));
		numbersObject.transform.SetParent(numbersPanel.transform, false);
		Text numbers = numbersObject.GetComponent<Text>();
		numbers.font = font;
		numbers.fontSize = 18;
		numbers.alignment = TextAnchor.MiddleRight;
		numbers.color = new Color(1f, 1f, 1f, 0.9f);
		RectTransform numbersRect = numbersObject.GetComponent<RectTransform>();
		numbersRect.anchorMin = Vector2.zero;
		numbersRect.anchorMax = Vector2.one;
		numbersRect.offsetMin = new Vector2(10f, 0f);
		numbersRect.offsetMax = new Vector2(-10f, 0f);

		RangeUI ui = canvasObject.AddComponent<RangeUI>();
		ui.toggleButton = buttonObject.GetComponent<Button>();
		ui.toggleLabel = label;
		ui.helpLabel = help;
		ui.numbersLabel = numbers;
		ui.player = Object.FindFirstObjectByType<PlayerRig>();
	}
}
