using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class ProjectileScript : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField] private int damageAmount;
		[SerializeField] private bool canContinueAfterHit = true;
		[SerializeField] private float speed = 4f;

		[Header("Visuals")]
		[SerializeField] private float deathDestroyDelay = 0.33f;
		[SerializeField, Required] private Sprite deathSprite;

		private Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();
		private Rigidbody2D _rb;

		private Collider2D Col => _collider ??= GetComponent<Collider2D>();
		private Collider2D _collider;

		private SpriteRenderer SP => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
		private SpriteRenderer _spriteRenderer;

		[ShowInInspector, ReadOnly] private int _remainingDamage;
		[Space(2f)]
		[ShowInInspector, ReadOnly] private Vector2 _direction;
		[ShowInInspector, ReadOnly] private float _speed;

		public void Init(Vector2 direction)
		{
			_direction = direction.normalized; // compensate just in case

			_remainingDamage = damageAmount;

			_speed = speed;
			RB.velocity = _direction * speed;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			BalloonScript balloon = collision.collider.GetComponent<BalloonScript>();
			if (balloon == null) return;

			int spareDamage = balloon.Damage(_remainingDamage);
			if (canContinueAfterHit && spareDamage > 0)
			{
				_remainingDamage = spareDamage;
			}
			else
			{
				KillProjectile();
			}
		}

		private void KillProjectile()
		{
			Col.enabled = false;
			RB.constraints = RigidbodyConstraints2D.FreezeAll;
			SP.sprite = deathSprite;
			StartCoroutine(DeathRoutine());
		}

		private IEnumerator DeathRoutine()
		{
			yield return new WaitForSeconds(deathDestroyDelay);
			Destroy(gameObject);
		}
	}
}