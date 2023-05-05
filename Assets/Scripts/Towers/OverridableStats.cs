using System;
using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Towers
{
	/// <summary>
	/// This is used in a list as a bunch of (optionally implemented) values
	/// </summary>
	[Serializable]
	[InlineProperty]
	public class OverridableStats
	{
		[field: SerializeField] public Optional<float> RangeOverride { get; private set; }
		[field: SerializeField] public Optional<float> AttackSpeedOverride { get; private set; }

		[field: Space(5f)]
		[field: SerializeField] public Optional<Sprite> SpriteOverride { get; private set; }
		[field: SerializeField] public Optional<Color> ColorOverride { get; private set; } = new Optional<Color>(Color.white);
	}
}