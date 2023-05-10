using System.Collections;
using Scripts.Yarn;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
	public class CommonProjectileScript : MonoBehaviour
	{
		private Rigidbody2D _rb;
		private CircleCollider2D _col;
		private SpriteRenderer _spriteRenderer;

		private Projectile _projectile;

		public void Init(Projectile projectile, Vector2 throwDirection) // only called once
		{
			_rb = GetComponent<Rigidbody2D>();
			_col = GetComponent<CircleCollider2D>();
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
			_col.radius = _projectile.ColliderRadius;
			_spriteRenderer.sprite = _projectile.Sprite;
			_spriteRenderer.color = _projectile.Color;
		}

		private void FixedUpdate()
		{
			// cull if out of bounds
			const float cullClearance = 3f;
			if (_projectileAlive && (_projectile.MaxLifetime.Enabled && _startTime.TimeSince() > _projectile.MaxLifetime.Value
			                         || Camera.main.PointOutsideViewArea2D(transform.position.V2FromV3(), cullClearance)))
			{
				Cull();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!_projectileAlive) return;
			CommonYarnScript target = collision.collider.GetComponent<CommonYarnScript>();
			if (target == null) return;

			if (!target.CanBeDamaged) return;

			DealProjectileDamageToYarn(target);
		}

		private void DealProjectileDamageToYarn(CommonYarnScript target) // returns true if all damage absorbed
		{
			while (true)
			{
				int damage;
				if (!_projectile.MaxDamage.Enabled)
				{
					damage = int.MaxValue;
				}
				else
				{
					damage = Mathf.Min(_projectile.MaxDamage.Value, _projectileDamageLeft); // cap under remaining damage points

					if (_projectile.ShellDurabilityType == Projectile.ShellDurability.Rigid && _projectile.MaxDamagePerCollision.Enabled) // check if rigid first bc fragility means maxdamage should be off
					{
						damage = Mathf.Min(damage, _projectile.MaxDamagePerCollision.Value); // cap under max damaage per hit
					}
				}

				_projectileDamageLeft -= target.Damage(damage, _projectile.TargetImpactType);

				if (_projectileDamageLeft <= 0 || _projectile.ShellDurabilityType == Projectile.ShellDurability.Fragile)
				{
					if (_projectile.BelowProjectile.Enabled)
					{
						ApplyProjectile(_projectile.BelowProjectile.Value);

						if (_projectile.BelowProjectileStackType == Projectile.ProjectileStackType.CanDamageWithBoth)
						{
							continue;
						}
					}
					else
					{
						StartCoroutine(KillProjectile());
					}
				}

				break;
			}
		}

		private IEnumerator KillProjectile()
		{
			_projectileAlive = false;

			_col.enabled = false;
			_rb.constraints = RigidbodyConstraints2D.FreezeAll;

			if (_projectile.DeathEffect.Enabled)
			{
				yield return _projectile.DeathEffect.Value.ApplyEffectToSpriteRenderer(_spriteRenderer);
			}
			Cull();
		}

		private void Cull()
		{
			Destroy(gameObject);
		}
	}
}