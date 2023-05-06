using Tools.Types;
using UnityEngine;

namespace Scripts.SOSystem
{
	[CreateAssetMenu(menuName = "Scriptable/YarnBaseLayer", fileName = "New YarnBaseLayer")]
	public class YarnBaseLayer : YarnLayer
	{
		public YarnValues baseValues = new YarnValues
		{
			speed = 1.25f,
			health = 1,
			damageAbsorptionCap = new Optional<int>(1),
			damagePassthroughType = DamagePassthroughType.Penetrable,
			color = Color.white,
			sprite = null,
			deathSprite = null,
			deathTime = 0
		};
	}
}