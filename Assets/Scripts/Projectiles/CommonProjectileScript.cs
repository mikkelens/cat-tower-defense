using System.Collections;
using Scripts.Yarn;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class CommonProjectileScript : MonoBehaviour
	{
		private Rigidbody2D _rb;
		private Collider2D _col;
		private SpriteRenderer _spriteRenderer;

		private Projectile _projectile;

		public void Init(Projectile projectile, Vector2 throwDirection) // only called once
		{
			_rb = GetComponent<Rigidbody2D>();
			_col = GetComponent<Collider2D>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

			_throwDirection = throwDirection;
			_startTime = Time.time;
			_projectileAlive = true;

			ApplyProjectile(projectile);
		}

		private Vector2 _throwDirection;
		private float _startTime;
		private int _projectileDamageLeft;
		private bool _projectileAlive;

		private void ApplyProjectile(Projectile projectile) // possibly used multiple times
		{
			if (projectile == null) Debug.LogError("Projectile missing when trying to assign it at instantiation Init()");
			_projectile = projectile;
			_projectileDamageLeft = _projectile.MaxDamage.Enabled ? _projectile.MaxDamage.Value : int.MaxValue;
			_rb.velocity = _throwDirection * _projectile.TravelSpeed;
			_spriteRenderer.sprite = _projectile.Sprite;
		}

		private void FixedUpdate()
		{
			// cull if out of bounds
			const float cullClearance = 3f;
			if (_projectileAlive && (_projectile.MaxLifetime.Enabled && _startTime.TimeSince() > _projectile.MaxLifetime.Value
			                         || Camera.main.PointOutsideViewArea2D(transform.position.V2FromV3(), cullClearance)))
			{
				CullImmediate();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!_projectileAlive) return;
			YarnScript target = collision.collider.GetComponent<YarnScript>();
			if (target == null) return;

			bool newProjectileSplinter;
			do
			{
				newProjectileSplinter = false;
				if (!target.CanBeDamaged) break;

				_projectileDamageLeft -= DealProjectileDamageToYarn(target);

				if (_projectile.ImpactType != Projectile.ProjectileImpactType.Fragile && _projectileDamageLeft > 0) continue;
				if (_projectile.BelowProjectile.Enabled)
				{
					if (_projectile.BelowProjectile.Value == null) Debug.LogError($"Below Projectile of {_projectile.name} is null!");

					ApplyProjectile(_projectile.BelowProjectile.Value);
					newProjectileSplinter = true;
				}
				else
				{
					KillProjectile();
				}
			} while (newProjectileSplinter);
		}

		private int DealProjectileDamageToYarn(YarnScript target) // returns true if all damage absorbed
		{
			if (!_projectile.MaxDamage.Enabled) return target.Damage(int.MaxValue);

			int damage = Mathf.Min(_projectile.MaxDamage.Value, _projectileDamageLeft); // cap under remaining damage points

			if (_projectile.ImpactType == Projectile.ProjectileImpactType.Rigid && _projectile.MaxDamagePerCollision.Enabled)
				damage = Mathf.Min(damage, _projectile.MaxDamagePerCollision.Value); // cap under max damaage per hit
			return target.Damage(damage);
		}

		private void KillProjectile()
		{
			_projectileAlive = false;

			_col.enabled = false;
			_rb.constraints = RigidbodyConstraints2D.FreezeAll;

			_spriteRenderer.sprite = _projectile.DeathSprite;
			StartCoroutine(CullAfterDelay(_projectile.DeathCullDelay));
		}

		private IEnumerator CullAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			CullImmediate();
		}
		private void CullImmediate()
		{
			Destroy(gameObject);
		}
	}
}