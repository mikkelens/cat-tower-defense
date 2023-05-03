using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class BalloonScript : MonoBehaviour
	{
		[SerializeField, Min(0f)] private float speed = 1.2f;
		[SerializeField, Min(1)] private int health = 1;

		[ShowInInspector, ReadOnly] private List<Transform> _targets;
		public void Init(List<Transform> targets)
		{
			_targets = targets;
			StartCoroutine(PathFollowRoutine());
		}

		public void Damage(int amount = 1)
		{

		}

		private SpriteRenderer _spriteRenderer;

		private void Kill()
		{
			StopAllCoroutines();

		}

		private Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();
		private Rigidbody2D _rb;

		private IEnumerator PathFollowRoutine()
		{
			foreach (Transform target in _targets)
			{
				Vector2 targetPos = target.position.V2FromV3();
				const float minDistance = 0.0001f;
				while (Vector2.Distance(RB.position, targetPos) <= minDistance)
				{
					Vector2 newPos = Vector2.MoveTowards(RB.position, targetPos, speed * Time.deltaTime);
					RB.MovePosition(newPos);
					yield return null; // waits one frame: https://forum.unity.com/threads/coroutine-wait-x-frames-not-seconds.550168/
				}
			}
		}
	}
}