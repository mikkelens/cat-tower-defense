using System.Collections;
using UnityEngine;

namespace Scripts
{
	public class BasicEffectScript : MonoBehaviour
	{
		public void Init(Effect data)
		{
			StartCoroutine(LifetimeRoutine(data));
		}

		private IEnumerator LifetimeRoutine(Effect data)
		{
			SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

			yield return data.Curve.Enabled
				? data.ApplyEffectToSpriteRenderer(spriteRenderer)
				: new WaitForSeconds(data.Duration);

			Cull();
		}

		private void Cull()
		{
			Destroy(gameObject);
		}
	}
}