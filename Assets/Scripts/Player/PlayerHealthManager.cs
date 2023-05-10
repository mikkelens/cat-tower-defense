using Sirenix.OdinInspector;
using TMPro;
using Tools.Types;
using UnityEngine;

namespace Scripts.Player
{
	public class PlayerHealthManager : Singleton<PlayerHealthManager>
	{
		[Header("Health settings")]
		[SerializeField] private int startHealth = 100;
		[SerializeField, Required] private TextMeshProUGUI textGUI;
		[SerializeField] private Optional<Color> deathColor = Color.red;

		private int _health;
		private int Health
		{
			get => _health;
			set
			{
				_health = value;
				textGUI.text = _health.ToString();
				if (deathColor.Enabled && _health <= 0) textGUI.color = deathColor.Value;
			}
		}

		private void Start()
		{
			Health = startHealth;
		}

		public void Damage(int amount)
		{
			Health -= Mathf.Min(amount, Health); // outcome will be zero or more
		}
	}
}