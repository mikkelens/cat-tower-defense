using System;
using System.Collections;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts
{
	[Serializable]
	public struct Effect // inspector field category
	{
		[field: Min(0f)]
		[field: SerializeField] public float Duration { get; set; }
		[field: SerializeField] public Sprite Sprite { get; private set; }
		[field: SerializeField] public Color Color { get; set; }
		[field: SerializeField] public float Size { get; set; }
		[field: SerializeField] public Optional<AnimationCurve> Curve { get; set; }

		public static Effect LinearFade(float duration = 0.5f)
		{
			return new Effect
			{
				Duration = duration,
				Color = Color.white,
				Size = 1f,
				Curve = AnimationCurve.Linear(0f, 1f, 1f, 0f).AsDisabled(),
			};
		}

		public IEnumerator ApplyEffectToSpriteRenderer(SpriteRenderer spriteRenderer)
		{
			spriteRenderer.sprite = Sprite;
			spriteRenderer.color = Color;
			spriteRenderer.transform.localScale = Size.ToCubeV3();
			spriteRenderer.sortingLayerName = "Effect";

			yield return Curve.Enabled ? AnimateDeathEffect(spriteRenderer.transform) : new WaitForSeconds(Duration);
		}

		private IEnumerator AnimateDeathEffect(Transform animTransform)
		{
			Vector3 baseSize = Size.ToCubeV3();
			float startTime = Time.time;
			while (startTime.TimeSince() <= Duration)
			{
				float t = startTime.TimeSince() / Duration;
				animTransform.localScale = Curve.Value.Evaluate(t) * baseSize;
				yield return new WaitForSeconds(Time.deltaTime);
			}
		}
	}
}