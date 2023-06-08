using System;
using JetBrains.Annotations;
using Unity.Properties;
using UnityEngine;

namespace Scripts.Yarn
{
	/// <summary>
	/// Common type, existing either as "Override" or "Base" yarn layer
	/// </summary>
	public abstract class YarnLayer : ScriptableObject
	{
		public YarnValues GetLayerValuesRecursively()
		{
			return this switch
			{
				YarnBaseLayer baseLayer => baseLayer.baseValues,
				YarnOverrideLayer overrideLayer => overrideLayer
					.ApplyOverridesToValues(overrideLayer.belowLayer.GetLayerValuesRecursively()),
				_ => throw new InvalidContainerTypeException(GetType())
			};
		}

		[Serializable]
		[UsedImplicitly] // other values than ones tested against by if statements could be selected in unity editor
		public enum Surface
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