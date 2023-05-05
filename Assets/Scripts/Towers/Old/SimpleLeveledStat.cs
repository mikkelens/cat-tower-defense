using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Towers.Old
{
	[Serializable]
	public struct SimpleLeveledStat<T>
	{
		[field: SerializeField] public T BaseValue { get; private set; }

		[field: SerializeField, ListDrawerSettings(ShowIndexLabels = true)]
		public List<Optional<T>> Upgrades { get; private set; }
		// optional because a level can have an upgrade that doesn't change anything

		public SimpleLeveledStat(T value)
		{
			BaseValue = value;
			_prevValue = value;
			_level = -1; // base value
			_levelHasChanged = false;
			Upgrades = new List<Optional<T>>();
		}

		private bool _levelHasChanged;
		private T _prevValue; // cache

		private T PrevValue
		{
			get => _prevValue;
			set
			{
				_levelHasChanged = false;
				_prevValue = value;
			}
		}
		private int _level;
		public int Level
		{
			readonly get => _level;
			set
			{
				if (value < -1) value = -1; // limit value to a minimum of -1, representing our base value
				if (value != _level) _levelHasChanged = true; // for caching
				_level = value;
			}
		}
		public T UpdatedValue
		{
			get
			{
				if (!_levelHasChanged) return PrevValue; // cache reuse

				if (Level == -1) return _prevValue = BaseValue;

				if (Upgrades.IsEmpty()) return PrevValue;

				if (Upgrades.Count > Level)
				{
					// level *not* too high

					Optional<T> exactUpgrade = Upgrades[Level];
					if (exactUpgrade.Enabled) return PrevValue = exactUpgrade.Value; // exact upgrade found, simplest solution

					for (int i = Level; i >= 0; i--)
					{
						// iterate down from level until valid value is found
						Optional<T> upgrade = Upgrades[i];
						if (upgrade.Enabled) return PrevValue = upgrade.Value; // return highest valid value under level
					}
				}
				else
				{
					// level *is* too high
					for (int i = Upgrades.Count - 1; i >= 0; i--)
					{
						// iterate down in upgrades untill valid value is found
						Optional<T> upgrade = Upgrades[i];
						if (upgrade.Enabled) return upgrade.Value; // return highest valid value in real range
					}
				}

				return BaseValue; // just in case we couldn't find an appropriate value
			}
		}
	}
}