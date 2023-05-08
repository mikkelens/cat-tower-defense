using System.Collections;
using Scripts.Yarn;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	// todo: remove this and replace with CommonProjectileScript
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class OldProjectileScript : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField, Min(1)] private int damageAmount = 3;
		[ShowIf("@damageAmount > 1")]
		[SerializeField] private Optional<int> maxDamagePerHit = new Optional<int>(2);
		[SerializeField] private bool canContinueAfterHit = true;
		[SerializeField] private float speed = 4f;
		[SerializeField] private Optional<float> maxLifetime = new Optional<float>(3f, true);

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

			_alive = true;
			_initTime = Time.time;
		}

		private bool _alive;
		private float _initTime;

		private void Update()
		{
			if (_alive && (Camera.main.PointOutsideViewArea2D(transform.position.V2FromV3(), 2f)
			               || (maxLifetime.Enabled && _initTime.TimeSince() > maxLifetime.Value)))
			{
				KillProjectile();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!_alive) return;
			YarnScript yarn = collision.collider.GetComponent<YarnScript>();
			if (yarn == null) return;

			int cappedDamage = maxDamagePerHit.Enabled ? Mathf.Min(_remainingDamage, maxDamagePerHit.Value) : _remainingDamage;
			int dealtDamage = yarn.Damage(cappedDamage);
			_remainingDamage -= dealtDamage;
			if (!canContinueAfterHit || _remainingDamage < 1)
			{
				KillProjectile();
			}
		}

		private void KillProjectile()
		{
			_alive = false;
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