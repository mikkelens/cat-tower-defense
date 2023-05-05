using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Towers
{
	/// <summary>
	/// All-enabled values
	/// </summary>
	[Serializable]
	public class BaseStats
	{
		[field: SerializeField] public float Range { get; private set; } = 2.05f;
		[field: SerializeField] public float AttackSpeed { get; private set; } = 1f;
		[field: SerializeField] public Color Color { get; private set; } = Color.white;

		[field: HorizontalGroup("Sprite")]
		[field: SerializeField] public Sprite Sprite { get; set; } // may be filled by towerscript

		#if UNITY_EDITOR
		[HorizontalGroup("Sprite")]
		[EnableIf("@Sprite != FindSpriteFromRenderer")]
		[Button("Find from SpriteRenderer")] public void ApplySpriteFromRenderer()
		{
			Sprite = FindSpriteFromRenderer;
			// EditorUtility.SetDirty(Selection.activeTransform.GetComponent<TowerScript>()); // not needed it seems
		}
		[UsedImplicitly] public Sprite FindSpriteFromRenderer => Selection.activeTransform.GetComponentInChildren<SpriteRenderer>().sprite;
		#endif
	}
}