using System;
using System.Diagnostics;

namespace positron
{
	public class HealthMeter : SpriteBase
	{
		Player _Player;
		Player Player { get { return _Player; } set { _Player = value; } }
		public HealthMeter (RenderSet render_set, double x, double y, Player player):
			base(render_set, x, y, Texture.Get("sprite_health_meter_atlas"))
		{
			_Player = player;
			player.HealthChanged += OnHealthChanged;
		}
		public void OnHealthChanged (object sender, HealthChangedEventArgs e)
		{
			int region_idx = (Player.HealthMax - e.HealthWas); // Hard-coded
			int next_idx = region_idx + 1; // Hard-coded
			if (e.HealthWas != e.HealthNow) {
				int step = e.HealthWas > e.HealthNow ? 6 : -6;
				_AnimationCurrent = new SpriteAnimation(Texture, 100, region_idx, region_idx + step,  region_idx + 2 * step, next_idx);
				_AnimationFrameIndex = 0;
				_FrameTimer.Restart ();
			}
		}
	}
}

