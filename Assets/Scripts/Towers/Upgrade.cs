using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Towers
{
	[Serializable]
	[InlineProperty]
	public struct Upgrade
	{
		// [field: SerializeField] public int Cost { get; private set; }
		// [field: Space]
		[field: SerializeField, HideLabel] public OverridableStats Overrides { get; private set; }
	}
}