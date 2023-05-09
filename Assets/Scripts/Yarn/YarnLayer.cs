using System;
using Sirenix.OdinInspector;
using Tools.Types;
using Unity.Properties;
using UnityEngine;

namespace Scripts.Yarn
{
	/// <summary>
	/// Common type, existing either as "Override" or "Base" yarn layer
	/// </summary>
	public abstract class YarnLayer : ScriptableObject
	{
		[Serializable]
		public struct YarnValues
		{
			[Min(0f)]
			public float speed;
			[Min(1)]
			public int health;
			[Min(0), EnableIf("@health > 1")]
			public Optional<int> damageAbsorptionCap;
			public DamagePassthroughType damagePassthroughType;

			[Space]
			public Color color;
			[Required]
			public Sprite sprite;

			[Space] // never overridden
			[Required]
			public Sprite deathSprite;
			public float deathTime;
		}

		public YarnValues GetLayerValuesRecursively()
		{
			return this switch
			{
				YarnBaseLayer baseLayer => baseLayer.baseValues,
				YarnOverrideLayer overrideLayer => overrideLayer.ApplyOverridesToValues(overrideLayer.belowLayer.GetLayerValuesRecursively()),
				_ => throw new InvalidContainerTypeException(GetType())
			};
		}

		[Serializable]
		public enum DamagePassthroughType
		{
			Penetrable, // let projectile damage affect below layers
			Impenetrable // absorb layer damage, below layers unaffected
		}
	}
}