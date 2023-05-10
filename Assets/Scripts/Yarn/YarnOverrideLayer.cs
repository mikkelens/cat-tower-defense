using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Yarn
{
	[CreateAssetMenu(menuName = "Scriptable/YarnOverrideLayer", fileName = "New YarnOverrideLayer")]
	public class YarnOverrideLayer : YarnLayer
	{
		[Header("Below Reference")]
		[SerializeField, Required] public YarnLayer belowLayer;

		[Header("Overrides")]
		[ValidateInput("@speedOverride.Enabled == false || speedOverride.Value > 0f", "Speed cannot be less than or equal to zero!")]
		public Optional<float> speedOverride = new Optional<float>(2f);
		[ValidateInput("@healthOverride.Enabled == false || healthOverride.Value >= 1", "Health override cannot be less than 1!")]
		public Optional<int> healthOverride = new Optional<int>(3);
		[ValidateInput("@damageAbsorptionCapOverride.Enabled == false || damageAbsorptionCapOverride.Value >= 1", "Damage absorption cap override cannot be less than 1!")]
		public Optional<int> damageAbsorptionCapOverride = new Optional<int>(2);
		public Optional<Surface> damagePassthroughTypeOverride; // always valid
		[Space]
		public Optional<Color> colorOverride = new Optional<Color>(Color.white);
		[ValidateInput("@spriteOverride.Enabled == false || spriteOverride.Value != null", "You need to assign a sprite here!")]
		public Optional<Sprite> spriteOverride;

		public YarnValues ApplyOverridesToValues(YarnValues values)
		{
			if (speedOverride.Enabled) values.speed = speedOverride.Value;
			if (healthOverride.Enabled) values.health = healthOverride.Value;
			if (damageAbsorptionCapOverride.Enabled) values.damageAbsorptionCap = new Optional<int>(damageAbsorptionCapOverride.Value, true);
			if (damagePassthroughTypeOverride.Enabled) values.surface = damagePassthroughTypeOverride.Value;

			if (colorOverride.Enabled) values.color = colorOverride.Value;
			if (spriteOverride.Enabled) values.sprite = spriteOverride.Value;
			return values;
		}
	}
}