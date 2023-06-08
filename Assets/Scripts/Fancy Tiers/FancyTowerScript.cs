using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;

namespace Fancy_Tiers
{
	[Serializable, InlineProperty]
	public class FloatStat
	{
		[field: SerializeField, HideLabel] public float BaseValue { get; set; }

		public FloatStat(float baseValue)
		{
			BaseValue = baseValue;
			_modifiers = new List<FloatModifier>();
			RecalculateValue();
		}
		public static implicit operator FloatStat(float value) => new FloatStat(value);

		private List<FloatModifier> _modifiers;

		public void AddModifier(FloatModifier modifier)
		{
			_modifiers.Add(modifier);
			RecalculateValue();
		}
		public bool TryRemoveModifier(FloatModifier modifier)
		{
			bool removed = _modifiers.Remove(modifier);
			if (removed) RecalculateValue();
			return removed;
		}
		public void ClearModifiers()
		{
			_modifiers.Clear();
			RecalculateValue();
		}

		public float CalculatedValue { get; private set; }
		public void RecalculateValue()
		{
			float value = BaseValue;
			foreach (FloatModifier modifier in _modifiers) // should start from bottom of list
			{
				switch (modifier.Type)
				{
					case ModifierType.ValueAdd:
						value += modifier.Amount;
						break;
					case ModifierType.FlatFractionAdd:
						value += modifier.Amount * BaseValue;
						break;
					case ModifierType.StackedFractionAdd:
						value += modifier.Amount * value;
						break;
					case ModifierType.StackedMultiply:
						value *= modifier.Amount;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			CalculatedValue = value;
		}
	}

	[Serializable]
	public enum ModifierType
	{
		// base relative
		ValueAdd, // values that simply add an amount
		FlatFractionAdd, // adds amount relative to base
		// whole relative, calculations starting "from base" upwards (in modifier (nested) list order)
		StackedFractionAdd, // adds amount relative to whole (stack)
		StackedMultiply, // multiplies by amount relative to whole (stack)
	}
	[Serializable]
	public struct FloatModifier
	{
		[field: SerializeField] public ModifierType Type { get; private set; }
		[field: SerializeField] public float Amount { get; private set; }
	}
	[Serializable]
	public class Upgrade
	{
		[SerializeField, UsedImplicitly] public string upgradeName = "Unnamed Tier";

		[field: SerializeField] public int Cost { get; private set; } = 5;

		[field: SerializeField] public Optional<FloatModifier> ViewRangeModifier { get; private set; }
		[field: SerializeField] public Optional<FloatModifier> AttackSpeedModifier { get; private set; }
		[field: SerializeField] public Optional<FloatModifier> AttackDamageModifier { get; private set; }
	}

	[Serializable]
	public class UpgradePath
	{
		private const string DefaultPathName = "Unnamed Path";
		[SerializeField, UsedImplicitly] public string pathName = DefaultPathName;


		private string UpgradesName => pathName != DefaultPathName ? pathName + " Tiers" : nameof(UnlockableUpgrades);
		[field: LabelText("@" + nameof(UpgradesName))]
		[field: ListDrawerSettings(ListElementLabelName = nameof(Upgrade.upgradeName))]
		[field: SerializeField] public List<Upgrade> UnlockableUpgrades { get; private set; }

		[field: SerializeField, ReadOnly] public int Level { get; private set; } = -1;
		[ButtonGroup("LevelLiveModify")]
		[Button("Level Down"), EnableIf(nameof(CanLevelDown))] public void LevelDown()
		{
			Level--;
			ReadjustUnlockedUpgrades();
		}
		public bool CanLevelDown => Level - 1 >= -1;
		[ButtonGroup("LevelLiveModify")]
		[Button("Level Up"), EnableIf(nameof(CanLevelUp))] public void LevelUp()
		{
			Level++;
			ReadjustUnlockedUpgrades();
		}
		public bool CanLevelUp => Level + 1 < UnlockableUpgrades.Count;
		public void SetLevel(int value)
		{
			Level = value;
			ReadjustUnlockedUpgrades();
		}

		[field: SerializeField, HideInInspector] public List<Upgrade> UnlockedUpgrades { get; private set; } = new List<Upgrade>();

		private void ReadjustUnlockedUpgrades()
		{
			List<Upgrade> unlocked = new List<Upgrade>();
			for (int i = 0; i <= Level; i++) // assumes Level cannot reach outside unloackableupgrade list bounds
			{
				unlocked.Add(UnlockableUpgrades[i]);
			}
			UnlockedUpgrades = unlocked;
		}
	}
	[Serializable]
	public class UpgradableStats
	{
		[field: SerializeField] public int BuyCost { get; private set; } = 10; // should be set by shop in future?

		// active stats
		[field: SerializeField] public FloatStat ViewRange { get; private set; } = 1.25f;
		[field: SerializeField] public FloatStat AttackSpeed { get; private set; } = 2f;
		[field: SerializeField] public FloatStat AttackDamage { get; private set; } = 1f;

		public void ApplyModifiersFromUpgrades()
		{
			if (UpgradePaths.IsEmpty()) return;

			ViewRange.ClearModifiers();
			AttackSpeed.ClearModifiers();
			AttackDamage.ClearModifiers();

			// apply all modifiers
			int highestTier = UpgradePaths.Select(path => path.UnlockedUpgrades.Count - 1).Prepend(-1).Max();
			for (int i = 0; i <= highestTier; i++) // important that we go tier by tier, then path by path (multiplicative stacking)
			{
				foreach (Upgrade tierUpgrade in UpgradePaths.Select(path => path.UnlockedUpgrades[i]))
				{
					if (tierUpgrade.ViewRangeModifier.Enabled)
						ViewRange.AddModifier(tierUpgrade.ViewRangeModifier.Value);
					if (tierUpgrade.AttackSpeedModifier.Enabled)
						AttackSpeed.AddModifier(tierUpgrade.AttackSpeedModifier.Value);
					if (tierUpgrade.AttackDamageModifier.Enabled)
						AttackDamage.AddModifier(tierUpgrade.AttackDamageModifier.Value);
				}
			}
		}

		[field: ListDrawerSettings(ShowFoldout = true, ListElementLabelName = nameof(UpgradePath.pathName))]
		[field: SerializeField] public List<UpgradePath> UpgradePaths { get; private set; }

		public const float SellCostFraction = 0.7f;
		[ShowInInspector, ReadOnly] public int GetTotalSellCost
		{
			get
			{
				int upgradeCost = 0;
				foreach (UpgradePath path in UpgradePaths.Where(path => path.Level != -1))
				{
					for (int i = 0; i < path.Level; i++)
					{
						Upgrade withinLevel = path.UnlockableUpgrades[i];
						upgradeCost += withinLevel.Cost;
					}
				}

				int totalCost = BuyCost + upgradeCost;
				int sellCost = Mathf.RoundToInt(totalCost * SellCostFraction);
				return sellCost;
			}
		}
	}

	public class FancyTowerScript : MonoBehaviour
	{
		[SerializeField] private UpgradableStats stats;
	}
}