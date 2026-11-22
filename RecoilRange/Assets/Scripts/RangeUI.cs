using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One button and two lines of text. The button is the sample: press it and the
/// same shot stops feeling like a shot.
/// </summary>
public class RangeUI : MonoBehaviour
{
	public Button toggleButton;
	public Text toggleLabel;
	public Text helpLabel;
	public Text numbersLabel;
	public PlayerRig player;

	void Start()
	{
		if (toggleButton != null)
		{
			toggleButton.onClick.AddListener(Flip);
		}
		if (helpLabel != null)
		{
			helpLabel.text = "WASD move, Q and E turn, Space or left click to fire";
		}
		Refresh();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Flip();
		}
		ShowNumbers();
	}

	/// <summary>
	/// The numbers are on screen on purpose. The layer is not a mood, it is four
	/// values, and a visitor can see them change the moment the switch flips.
	/// </summary>
	void ShowNumbers()
	{
		FeelSettings feel = FeelSettings.Instance;
		if (feel == null || numbersLabel == null)
		{
			return;
		}

		if (feel.juiceOn)
		{
			float pushed = player != null ? player.LastRecoilUnits : 0f;
			numbersLabel.text = string.Format(
				"recoil {0:0.00} u   shake {1:0.00} u / {2} ms   hitstop {3} ms   flash {4} ms   punch {5:0.00}x / {6} ms",
				pushed, feel.shakeAmplitude, feel.shakeMs, feel.hitstopMs, feel.flashMs, feel.hitPunch, feel.punchMs);
		}
		else
		{
			numbersLabel.text = "recoil 0.00 u   shake 0.00 u / 0 ms   hitstop 0 ms   flash 0 ms   punch 1.00x / 0 ms";
		}
	}

	void Flip()
	{
		FeelSettings feel = FeelSettings.Instance;
		feel.juiceOn = !feel.juiceOn;
		Refresh();
	}

	void Refresh()
	{
		FeelSettings feel = FeelSettings.Instance;
		if (feel == null || toggleLabel == null)
		{
			return;
		}

		toggleLabel.text = feel.juiceOn
			? "Game feel: ON  (Tab)"
			: "Game feel: OFF  (Tab)";
		ColorBlock colors = toggleButton.colors;
		colors.normalColor = feel.juiceOn
			? new Color(0.17f, 0.45f, 0.30f)
			: new Color(0.30f, 0.17f, 0.17f);
		toggleButton.colors = colors;
	}
}
