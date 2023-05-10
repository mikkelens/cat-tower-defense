using System;
using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Yarn
{
	[Serializable]
	public struct YarnValues
	{
		[Min(0f)]
		public float speed;
		[Min(1)]
		public int health;
		[EnableIf("@health > 1"), ValidateInput(nameof(DamageAbsorptionMinValidation), "Absorption cap must be bigger than zero and less than health.")]
		public Optional<int> damageAbsorptionCap;
		private bool DamageAbsorptionMinValidation => !damageAbsorptionCap.Enabled || (damageAbsorptionCap.Value > 0 && damageAbsorptionCap.Value < health);

		[FormerlySerializedAs("surfaceType")] [FormerlySerializedAs("surfaceStrength")] [FormerlySerializedAs("damagePassthroughType")] public YarnLayer.Surface surface;

		[Space]
		public Color color;
		[Required]
		public Sprite sprite;

		[Space] // never overridden
		public Optional<Effect> deathEffect;
	}
}