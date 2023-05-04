using System;
using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.FloatUpgradeSystem
{
	[Serializable]
	public class FloatIncreaseModifier
	{
		[SerializeField, HideLabel] private Optional<float> levelStackModifier = 0.25f;

		private int _level; // 0 by default
		public int Level
		{
			get => _level;
			set
			{
				_needToRecalculate = true;
				_level = value;
			}
		}

		private bool _needToRecalculate = true;
		private float _oldValue;
		public float CalculatedValue(float baseValue)
		{
			if (!levelStackModifier.Enabled) return baseValue;
			if (!_needToRecalculate) return _oldValue;
			float levelAddition = Level * levelStackModifier.Value * baseValue;
			return _oldValue = baseValue + levelAddition;
		}
	}
}