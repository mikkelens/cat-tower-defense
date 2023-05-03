using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class BalloonScript : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField, Min(0f)] private float speed = 1.2f;
		[SerializeField, Min(1)] private int health = 1;

		[Header("Visuals")]
		[SerializeField] private float deathDestroyDelay = 0.5f;
		[SerializeField, Required] private Sprite deathSprite;

		[ShowInInspector, ReadOnly] private List<Transform> _targets;
		public void Init(List<Transform> targets)
		{
			_health = health;
			_targets = targets;
			StartCoroutine(PathFollowRoutine());
		}

		private int _health;
		public int Damage(int amount = 1)
		{
			_health -= amount;
			if (_health <= 0)
			{
				Kill();
			}
			return Mathf.Min(_health, 0); // will return damage that was not used
		}

		private SpriteRenderer _spriteRenderer;
		private SpriteRenderer SP => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

		private void Kill()
		{
			StopAllCoroutines();
			SP.sprite = deathSprite;
			StartCoroutine(DeathRoutine());
		}

		private IEnumerator DeathRoutine()
		{
			yield return new WaitForSeconds(deathDestroyDelay);
			Destroy(gameObject);
		}

		private Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();
		private Rigidbody2D _rb;

		// show for debugging purposes
		[ShowInInspector, ReadOnly] private Vector2 _targetPos;

		private IEnumerator PathFollowRoutine()
		{
			foreach (Transform target in _targets)
			{
				_targetPos = target.position.V2FromV3();
				const float minDistance = 0.0001f;
				while (Vector2.Distance(RB.position, _targetPos) > minDistance)
				{
					float deltaTime = Time.deltaTime;
					Vector2 newPos = Vector2.MoveTowards(RB.position, _targetPos, speed * deltaTime);
					RB.MovePosition(newPos);
					yield return new WaitForSeconds(deltaTime); // waits one frame: https://forum.unity.com/threads/coroutine-wait-x-frames-not-seconds.550168/
				}
			}
		}
	}
}