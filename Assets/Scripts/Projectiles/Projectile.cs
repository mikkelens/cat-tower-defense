using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Projectiles
{
	[CreateAssetMenu(menuName = "Scriptable/Projectile", fileName = "New Projectile")]
	public class Projectile : ScriptableObject
	{
		// movement and lifetime
		[field: Header("Movement, Size & Lifetime")]
		[field: Min(0f)]
		[field: SerializeField] public float TravelSpeed { get; private set; } = 4f;

		[field: Min(0f)]
		[field: SerializeField] public float ColliderRadius { get; private set; } = 0.25f;

		private bool MaxLifetimeValidation => MaxLifetime.Enabled == false || MaxLifetime.Value > 0f;
		[field: ValidateInput(nameof(MaxLifetimeValidation))]
		[field: SerializeField] public Optional<float> MaxLifetime { get; private set; } = new Optional<float>(4f);

		// damage
		private bool MaxDamageValidation => MaxDamage.Enabled == false || MaxDamage.Value > 0;
		[field: Header("Damage")]
		[field: ValidateInput(nameof(MaxDamageValidation), "Max Damage cannot be 0 or less.")]
		[field: SerializeField] public Optional<int> MaxDamage { get; private set; } = new Optional<int>(3, true);

		// durability
		[field: ShowIf(nameof(TargetImpactShow))] // reuse
		[field: SerializeField] public ShellDurability ShellDurabilityType { get; private set; } = ShellDurability.Rigid;
		public enum ShellDurability
		{
			Rigid, // keeps going if more damage left
			Fragile // always breaks on impact
		}

		// impact
		[field: ShowIf(nameof(TargetImpactShow))]
		[field: SerializeField] public TargetImpact TargetImpactType { get; private set; } = TargetImpact.SurfaceOnly;
		private bool TargetImpactShow => !MaxDamage.Enabled || MaxDamage.Value > 1; // would never be used if maxdamage was enabled as 1
		public enum TargetImpact
		{
			SurfaceOnly, // only damage outermost layer
			[UsedImplicitly] // selectable by editor
			Penetrating, // can damage more layers (assuming yarn hasnt disallowed it, see its surface type)
		}

		// rigidness optional specification
		private bool MaxDamagePerHitShow => !MaxDamageValidation || MaxDamage is { Enabled: true, Value: > 1 } && ShellDurabilityType == ShellDurability.Rigid;
		private bool MaxDamagePerHitValidation => !MaxDamagePerHitShow || !MaxDamagePerCollision.Enabled ||
		                                          (MaxDamagePerCollision.Value > 0 && (!MaxDamage.Enabled || MaxDamagePerCollision.Value < MaxDamage.Value));
		[field: ShowIf(nameof(MaxDamagePerHitShow))]
		[field: ValidateInput(nameof(MaxDamagePerHitValidation), "Piercing Damage Per Hit must be bigger than zero and less than Max Damage.")]
		[field: SerializeField] public Optional<int> MaxDamagePerCollision { get; private set; } = new Optional<int>(2);

		// optional below projectile
		private bool BelowProjectileShow => ShellDurabilityType == ShellDurability.Rigid;
		private bool BelowProjectileValidation => !BelowProjectileShow || BelowProjectile.Enabled == false || BelowProjectile.Value != null;
		[field: Header("Nested Projectile")]
		[field: ShowIf(nameof(BelowProjectileShow))]
		[field: ValidateInput(nameof(BelowProjectileValidation), "Below Projectile cannot be null.")]
		[field: SerializeField] public Optional<Projectile> BelowProjectile { get; private set; }

		private bool BelowProjectileStackShow => BelowProjectileShow && BelowProjectile.Enabled && BelowProjectileValidation;
		[field: ShowIf(nameof(BelowProjectileStackShow))]
		[field: SerializeField] public ProjectileStackType BelowProjectileStackType { get; private set; }
		public enum ProjectileStackType
		{
			CanDamageWithBoth,
			[UsedImplicitly] // selectable by editor
			OnlyOutermost,
		}

		// visuals
		[field: Header("Visuals")]
		[field: SerializeField, Required] public Sprite Sprite { get; private set; }
		[field: SerializeField] public Color Color { get; private set; } = Color.white;

		// death visuals
		private bool DeathEffectNullValidation => !DeathEffect.Enabled || DeathEffect.Value.Sprite != null;
		[field: Header("Death Visuals")]
		[field: ValidateInput(nameof(DeathEffectNullValidation), "If enabled, Death Effect should have a sprite!")]
		[field: SerializeField] public Optional<Effect> DeathEffect { get; private set; } = Effect.LinearFade().AsDisabled();

	}
}