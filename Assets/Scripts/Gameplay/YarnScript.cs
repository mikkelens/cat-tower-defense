using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Gameplay
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class YarnScript : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField, Min(0f)] private float speed = 1.2f;
		[SerializeField, Min(1)] private int health = 1;

		[Header("Visuals")]
		[SerializeField] private float deathDestroyDelay = 0.5f;
		[SerializeField, Required] private Sprite deathSprite;

		[ShowInInspector, ReadOnly] private List<Transform> _pathTargets;
		public void Init(List<Transform> targets)
		{
			_health = health;
			_pathTargets = targets;
			StartCoroutine(PathFollowRoutine());
		}

		private int _health;
		public int Damage(int incomingDamage)
		{
			if (_health == 0) return 0; // projectile can hit a dead thing if 2 projectiles hit it on the same physics update

			int dealtDamage = Mathf.Min(incomingDamage, _health); // can never deal more incomingDamage than maximum
			_health -= dealtDamage;
			if (_health == 0)
			{
				Kill();
			}
			return dealtDamage; // will return the incomingDamage that was actually dealt
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

		private IEnumerator PathFollowRoutine()
		{
			foreach (Transform pathTarget in _pathTargets)
			{
				Vector2 targetPos = pathTarget.position.V2FromV3();
				Vector2 targetDir = (targetPos - transform.position.V2FromV3()).normalized;
				Vector2 awayDir = -targetDir;
				transform.rotation = Quaternion.FromToRotation(Vector3.up, awayDir.V3FromV2()); // look away from direction, assuming default sprite dir is down (away from up)
				const float minDistance = 0.0001f;
				while (Vector2.Distance(RB.position, targetPos) > minDistance)
				{
					float deltaTime = Time.deltaTime;
					Vector2 newPos = Vector2.MoveTowards(RB.position, targetPos, speed * deltaTime);
					RB.MovePosition(newPos);
					yield return new WaitForSeconds(deltaTime); // waits one frame: https://forum.unity.com/threads/coroutine-wait-x-frames-not-seconds.550168/
				}
			}
		}
	}
}