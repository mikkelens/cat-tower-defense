using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Towers
{
	[Serializable]
	public class LeveledStats
	{
		[field: SerializeField, InlineProperty] public BaseStats BaseStats { get; private set; }

		[field: SerializeField, ListDrawerSettings(ShowIndexLabels = true, DefaultExpandedState = false)]
		public List<OverridableStats> UpgradeTiers { get; private set; }

		[SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;
		public void Init(SpriteRenderer renderer)
		{
			spriteRenderer = renderer; // important that this happens first
			UpdateStats();
		}

		[HorizontalGroup("LevelGroup", 0.75f)]
		[SerializeField, ReadOnly, PropertyOrder(-1)] private int level; // prevent direct editing
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
		[HorizontalGroup("LevelGroup")]
		[Button("-")] private void DecreaseLevel() => SetLevelInEditor(Level - 1);
		[HorizontalGroup("LevelGroup")]
		[Button("+")] private void IncreaseLevel() => SetLevelInEditor(Level + 1);
		private void SetLevelInEditor(int value)
		{
			EditorGUI.BeginChangeCheck();
			AssignSpriteRendererIfNecessary();
			Level = value;
			if (EditorGUI.EndChangeCheck())
			{
				Object target = Selection.activeObject;
				Undo.RecordObject(target, "Changed Tower Level");
				PrefabUtility.RecordPrefabInstancePropertyModifications(target);
			}
		}
		private void AssignSpriteRendererIfNecessary()
		{
			if (spriteRenderer != null) return;
			spriteRenderer = Selection.activeTransform.GetComponentInChildren<SpriteRenderer>();
			Debug.Log($"Assinged SpriteRenderer: {spriteRenderer} because it was missing.");
		}
		#endif

		public void UpdateStats()
		{
			Range = FindAppropriateValueForLevel(level, BaseStats.Range, UpgradeTiers.Select(x => x.RangeOverride).ToList());
			AttackSpeed = FindAppropriateValueForLevel(level, BaseStats.AttackSpeed, UpgradeTiers.Select(x => x.AttackSpeedOverride).ToList());

			Sprite = FindAppropriateValueForLevel(level, BaseStats.Sprite, UpgradeTiers.Select(x => x.SpriteOverride).ToList());
			Color = FindAppropriateValueForLevel(level, BaseStats.Color, UpgradeTiers.Select(x => x.ColorOverride).ToList());
			if (spriteRenderer != null)
			{
				if (Sprite != null) spriteRenderer.sprite = Sprite;
				else Debug.LogWarning($"Sprite from stats was null/empty, skipping sprite application..");
				spriteRenderer.color = Color;
			}
			else
			{
				Debug.LogWarning("SpriteRenderer on stats was unassigned?");
			}
		}

		public float Range { get; private set; }
		public float AttackSpeed { get; private set; }
		public Sprite Sprite { get; private set; }
		public Color Color { get; private set; }

		[CanBeNull] public T FindAppropriateValueForLevel<T>(int level, T baseValue, List<Optional<T>> upgrades)
		{
			if (level == -1 || upgrades.IsEmpty()) return baseValue;
			if (upgrades.Count > level)
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