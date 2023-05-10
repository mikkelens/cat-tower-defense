using System;
using Tools.Types;
using UnityEngine;

namespace Scripts.Projectiles
{
	[Serializable]
	public struct ProjectileDeathEffect // inspector field category
	{
		[field: SerializeField] public Sprite Sprite { get; private set; }
		[field: SerializeField] public Color Color { get; set; }
		[field: SerializeField] public float Size { get; set; }
		[field: Min(0f)]
		[field: SerializeField] public float Duration { get; set; }
		[field: SerializeField] public Optional<AnimationCurve> Curve { get; set; }
	}
}