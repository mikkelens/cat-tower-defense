using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Scripts.Projectiles;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Towers
{
	[Serializable]
	public class LeveledStats
	{
		public void Init(SpriteRenderer renderer)
		{
			spriteRenderer = renderer; // important that this happens first
			UpdateStats();
		}

		#region <level>
		[HorizontalGroup("Current/LevelGroup")]
		[SerializeField, ReadOnly] private int level;
		public int Level
		{
			get => level;
			set
			{
				if (value < -1) value = -1; // hard cap bottom
				if (value >= UpgradeTiers.Count) value = UpgradeTiers.Count - 1; // hard cap top

				if (value == level) return; // everything stays the same

				level = value;
				UpdateStats();
			}
		}
		#if UNITY_EDITOR
		private const int LevelButtonsWidth = 55;
		[UsedImplicitly] private bool CanDecreaseLevel => Level > -1;
		[HorizontalGroup("Current/LevelGroup", Width = LevelButtonsWidth), EnableIf("CanDecreaseLevel")]
		[Button("Level--")] private void DecreaseLevel() => SetLevelInEditor(Level - 1);
		[UsedImplicitly] private bool CanIncreaseLevel => Level < UpgradeTiers.Count - 1;
		[HorizontalGroup("Current/LevelGroup", Width = LevelButtonsWidth), EnableIf("CanIncreaseLevel")]
		[Button("Level++")] private void IncreaseLevel() => SetLevelInEditor(Level + 1);
		private void SetLevelInEditor(int value)
		{
			EditorGUI.BeginChangeCheck();
			Level = value;
			if (!EditorGUI.EndChangeCheck()) return;

			Object target = Selection.activeObject;
			Undo.RecordObject(target, "Changed Tower Level");
			PrefabUtility.RecordPrefabInstancePropertyModifications(target);
		}
		#endif
		#endregion </level>

		[FoldoutGroup("Current")]
		[ShowInInspector, ReadOnly]
		public float Range { get; private set; }
		[FoldoutGroup("Current")]
		[ShowInInspector, ReadOnly]
		public float AttackSpeed { get; private set; }
		[FoldoutGroup("Current")]
		[ShowInInspector, ReadOnly]
		public Sprite Sprite { get; private set; }
		[FoldoutGroup("Current")]
		[ShowInInspector, ReadOnly]
		public Color Color { get; private set; }
		[FoldoutGroup("Current")]
		[ShowInInspector, ReadOnly]
		public Projectile Projectile { get; private set; }

		[field: SerializeField] public BaseStats BaseStats { get; private set; }

		[ListDrawerSettings(ShowIndexLabels = true, DefaultExpandedState = false)]
		[field: SerializeField] public List<Upgrade> UpgradeTiers { get; private set; }

		[field: SerializeField, HideInInspector] public SpriteRenderer spriteRenderer;

		public void UpdateStats()
		{
			Range = FindAppropriateValueForLevel(BaseStats.Range, UpgradeTiers.Select(x => x.Overrides.RangeOverride).ToList());
			AttackSpeed = FindAppropriateValueForLevel(BaseStats.AttackSpeed, UpgradeTiers.Select(x => x.Overrides.AttackSpeedOverride).ToList());
			Projectile = FindAppropriateValueForLevel(BaseStats.Projectile, UpgradeTiers.Select(x => x.Overrides.ProjectileOverride).ToList());

			Sprite = FindAppropriateValueForLevel(BaseStats.Sprite, UpgradeTiers.Select(x => x.Overrides.SpriteOverride).ToList());
			Color = FindAppropriateValueForLevel(BaseStats.Color, UpgradeTiers.Select(x => x.Overrides.ColorOverride).ToList());
			if (spriteRenderer != null)
			{
				if (Sprite != null) spriteRenderer.sprite = Sprite;
				else Debug.LogWarning($"Sprite from stats was null/empty, skipping sprite application for {spriteRenderer.name}..");
				spriteRenderer.color = Color;
			}
			else
			{
				Debug.LogWarning("SpriteRenderer on stats was unassigned?");
			}
		}

		private T FindAppropriateValueForLevel<T>(T baseValue, List<Optional<T>> upgrades)
		{
			if (level == -1 || upgrades.IsEmpty()) return baseValue;
			if (level < upgrades.Count)
			{
				// level *not* too high
				Optional<T> exactUpgrade = upgrades[level];
				if (exactUpgrade.Enabled)
					return exactUpgrade.Value; // exact upgrade found, simplest solution

				for (int i = level; i >= 0; i--)
				{
					// iterate down from level until valid value is found
					Optional<T> upgrade = upgrades[i];
					if (upgrade.Enabled) return upgrade.Value; // return highest valid value under level
				}
			}
			else
			{
				// level *is* too high
				for (int i = upgrades.Count - 1; i >= 0; i--)
				{
					// iterate down in upgrades untill valid value is found
					Optional<T> upgrade = upgrades[i];
					if (upgrade.Enabled) return upgrade.Value; // return highest valid value in real range
				}
			}
			return baseValue; // just in case we couldn't find an appropriate value
		}
	}
}