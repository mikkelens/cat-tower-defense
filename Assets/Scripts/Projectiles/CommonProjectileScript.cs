using System;
using System.Collections;
using Scripts.Yarn;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class CommonProjectileScript : MonoBehaviour
	{
		private Projectile _projectile;

		public void Init(Projectile projectile)
		{
			if (projectile == null) Debug.LogError("Projectile missing when trying to assign it at instantiation Init()");
			_projectile = projectile;
		}

		private Rigidbody2D _rb;
		private Collider2D _col;
		private SpriteRenderer _spriteRenderer;

		private void Start()
		{
			_rb = GetComponent<Rigidbody2D>();
			_col = GetComponent<Collider2D>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

			_startTime = Time.time;
			_alive = true;
		}

		private float _startTime;
		private bool _alive;

		private void FixedUpdate()
		{
			// cull if out of bounds
			const float cullClearance = 3f;
			if (_alive && (_projectile.MaxLifetime.Enabled && _startTime.TimeSince() > _projectile.MaxLifetime.Value
			               || Camera.main.PointOutsideViewArea2D(transform.position.V2FromV3(), cullClearance)))
			{
				CullImmediate();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!_alive) return;
			YarnScript yarnScript = collision.collider.GetComponent<YarnScript>();
			if (yarnScript == null) return;

			// todo: finish method - damage yarn, possibly kill projectile
			int rawDamage = _projectile.MaxDamage.Enabled ? _projectile.MaxDamage.Value : int.MaxValue;
			int damageCapStack = _projectile
			int daamageCapPierce = _projectile.MaxDamagePerHitCap.Enabled
				? Mathf.Min(rawDamage, _projectile.MaxDamagePerHitCap.Value) : rawDamage;
			int dealtDamage = yarnScript.Damage(daamageCapPierce);
		}

		private void KillProjectile()
		{
			_alive = false;
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