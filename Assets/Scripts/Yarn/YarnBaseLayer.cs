using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Yarn
{
	[CreateAssetMenu(menuName = "Scriptable/YarnBaseLayer", fileName = "New YarnBaseLayer")]
	public class YarnBaseLayer : YarnLayer
	{
		[InlineProperty, HideLabel]
		public YarnValues baseValues = new YarnValues
		{
			speed = 1.25f,
			health = 1,
			damageAbsorptionCap = new Optional<int>(1),
			surface = Surface.Penetrable,
			color = Color.white,
			sprite = null,
			deathEffect = Effect.LinearFade(0.6f).AsDisabled()
		};
	}
}