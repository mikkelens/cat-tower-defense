using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Projectiles
{
	public class Projectile : ScriptableObject
	{
		// damage
		private bool MaxDamageValidation => MaxDamage.Enabled == false || MaxDamage.Value > 0;
		[field: ValidateInput(nameof(MaxDamageValidation), "Max Damage cannot be 0 or less.")]
		[field: SerializeField] public Optional<int> MaxDamage { get; private set; } = new Optional<int>(3, true);


		// impact type
		private bool ImpactTypeFragileValidation => !MaxDamageValidation  || ImpactType == ProjectileImpactType.Fragile && !MaxDamage.Enabled;
		private bool ImpactTypePiercingValidation => !MaxDamageValidation || ImpactType == ProjectileImpactType.Piercing && MaxDamage.Value != 1;
		[field: Space]
		[field: ValidateInput(nameof(ImpactTypeFragileValidation), "Projecitle cannot be fragile without an enabled Max Damage.")]
		[field: ValidateInput(nameof(ImpactTypePiercingValidation), "Projecitle cannot be piercing with a Max Damage of 1.")]
		[field: SerializeField] public ProjectileImpactType ImpactType { get; private set; } = ProjectileImpactType.Piercing;


		// movement and lifetime
		[field: SerializeField] public float TravelSpeed { get; private set; } = 4f;
		[field: SerializeField] public Optional<float> MaxLifetime { get; private set; } = new Optional<float>(4f);

		// piercing impact optional specification
		private bool MaxDamagePerHitShow => !MaxDamagePerHitValidation || MaxDamage is { Enabled: true, Value: > 1 } && ImpactType == ProjectileImpactType.Piercing;
		private bool MaxDamagePerHitValidation => !MaxDamagePerHitShow || !MaxDamagePerHitCap.Enabled ||
		                                          (MaxDamagePerHitCap.Value > 0 && (!MaxDamage.Enabled || MaxDamagePerHitCap.Value < MaxDamage.Value));
		[field: ShowIf(nameof(MaxDamagePerHitShow))]
		[field: ValidateInput(nameof(MaxDamagePerHitValidation), "Piercing Damage Per Hit must be bigger than zero and less than Max Damage.")]
		[field: SerializeField] public Optional<int> MaxDamagePerHitCap { get; private set; } = new Optional<int>(2);


		// optional below projectile
		private bool BelowProjectileShow => ImpactType == ProjectileImpactType.Piercing;
		private bool BelowProjectileValidation => !BelowProjectileShow || BelowProjectile.Enabled == false || BelowProjectile.Value != null;
		[field: Space]
		[field: ShowIf(nameof(BelowProjectileShow))]
		[field: ValidateInput(nameof(BelowProjectileValidation), "Below Projectile cannot be null.")]
		[field: SerializeField] public Optional<Projectile> BelowProjectile { get; private set; }


		private bool StackTypeShow => BelowProjectile.Enabled && BelowProjectileValidation;
		[field: ShowIf(nameof(StackTypeShow))]
		[field: SerializeField] public ProjectileStackType StackType { get; private set; } = ProjectileStackType.Stacking;


		// visuals
		[field: Space]
		[field: SerializeField, Required] public Sprite Sprite { get; private set; }
		[field: SerializeField, Required] public Sprite DeathSprite { get; private set; }
		[field: Min(0f)]
		[field: SerializeField] public float DeathCullDelay { get; private set; } = 0.33f;

		public enum ProjectileImpactType
		{
			Piercing,
			Fragile,
		}
		public enum ProjectileStackType
		{
			Stacking,
			Surface
		}
	}
}