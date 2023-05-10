using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Projectiles
{
	[CreateAssetMenu(menuName = "Scriptable/Projectile", fileName = "New Projectile")]
	public class Projectile : ScriptableObject
	{
		// movement and lifetime
		private bool MaxLifetimeValidation => MaxLifetime.Enabled == false || MaxLifetime.Value > 0f;
		[field: Header("Movement & Lifetime")]
		[field: Min(0f)]
		[field: SerializeField] public float TravelSpeed { get; private set; } = 4f;
		[field: ValidateInput(nameof(MaxLifetimeValidation))]
		[field: SerializeField] public Optional<float> MaxLifetime { get; private set; } = new Optional<float>(4f);

		// damage
		private bool MaxDamageValidation => MaxDamage.Enabled == false || MaxDamage.Value > 0;
		[field: Header("Damage")]
		[field: ValidateInput(nameof(MaxDamageValidation), "Max Damage cannot be 0 or less.")]
		[field: ValidateInput(nameof(ImpactTypeRigidValidation), "Projectile cannot be rigid with a Max Damage of 1.")]
		[field: SerializeField] public Optional<int> MaxDamage { get; private set; } = new Optional<int>(3, true);

		// impact type
		private bool ImpactTypeRigidValidation() => ImpactType != ProjectileImpactType.Rigid || !MaxDamage.Enabled || MaxDamage.Value > 1;
		// [field: ValidateInput(nameof(ImpactTypeRigidValidation), "Projectile cannot be rigid with a Max Damage of 1.")]
		[field: SerializeField] public ProjectileImpactType ImpactType { get; private set; } = ProjectileImpactType.Rigid;

		// piercing impact optional specification
		private bool MaxDamagePerHitShow => !MaxDamageValidation || MaxDamage is { Enabled: true, Value: > 1 } && ImpactType == ProjectileImpactType.Rigid;
		private bool MaxDamagePerHitValidation => !MaxDamagePerHitShow || !MaxDamagePerCollision.Enabled ||
		                                          (MaxDamagePerCollision.Value > 0 && (!MaxDamage.Enabled || MaxDamagePerCollision.Value < MaxDamage.Value));
		[field: ShowIf(nameof(MaxDamagePerHitShow))]
		[field: ValidateInput(nameof(MaxDamagePerHitValidation), "Piercing Damage Per Hit must be bigger than zero and less than Max Damage.")]
		[field: SerializeField] public Optional<int> MaxDamagePerCollision { get; private set; } = new Optional<int>(2);

		// optional below projectile
		private bool BelowProjectileShow => ImpactType == ProjectileImpactType.Rigid;
		private bool BelowProjectileValidation => !BelowProjectileShow || BelowProjectile.Enabled == false || BelowProjectile.Value != null;
		[field: Header("Nested Projectile")]
		[field: ShowIf(nameof(BelowProjectileShow))]
		[field: ValidateInput(nameof(BelowProjectileValidation), "Below Projectile cannot be null.")]
		[field: SerializeField] public Optional<Projectile> BelowProjectile { get; private set; }

		// visuals
		[field: Header("Visuals")]
		[field: SerializeField, Required] public Sprite Sprite { get; private set; }
		[field: SerializeField] public Color Color { get; private set; } = Color.white;

		// death visuals
		private bool DeathEffectNullValidation => !DeathEffect.Enabled || DeathEffect.Value.Sprite != null;

		[field: Header("Death Visuals")]
		[field: ValidateInput(nameof(DeathEffectNullValidation), "If enabled, Death Effect should have a sprite!")]
		[field: SerializeField] public Optional<ProjectileDeathEffect> DeathEffect { get; private set; } = new ProjectileDeathEffect
		{
			Size = 1f,
			Color = Color.white,
			Duration = 2f,
			Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
		}.AsDisabled();

		public enum ProjectileImpactType
		{
			Rigid, // keeps going if more damage left
			Fragile // always breaks on impact
		}
	}
}