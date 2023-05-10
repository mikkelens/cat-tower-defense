using System;
using JetBrains.Annotations;
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
		[UsedImplicitly] // other values than ones tested against by if statements could be selected in unity editor
		public enum DamagePassthroughType
		{
			Penetrable, // let projectile damage affect below layers
			Impenetrable // absorb layer damage, below layers unaffected
		}

		public int GetStackedHealthRecursively()
		{
			int layerHealth = GetLayerValuesRecursively().health;
			int belowHealth = 0;
			if (this is YarnOverrideLayer overrideLayer)
			{
				belowHealth = overrideLayer.belowLayer.GetStackedHealthRecursively();
			}

			return layerHealth + belowHealth;
		}
	}
}