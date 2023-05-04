using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Systems
{
	[Serializable]
	public class IncreaseableFloat
	{
		[SerializeField] public float baseValue;
		[SerializeField, InlineProperty] public FloatIncreaseModifier modifier;

		public IncreaseableFloat(float value)
		{
			baseValue = value;
		}

		public int Level
		{
			get => modifier.Level;
			set => modifier.Level = value;
		}
		public float Value => modifier.CalculatedValue(baseValue);
	}
}