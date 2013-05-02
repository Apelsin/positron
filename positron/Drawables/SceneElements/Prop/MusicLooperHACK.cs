using System;

namespace positron
{
    public class MusicLooperHACK : SpriteBase
    {
        public MusicLooperHACK (Scene scene, object loop_me):
            base(scene.HUD, -999, -999, Texture.DefaultTexture)
        {
            SetLoop(loop_me);
        }
        public void SetLoop (object loop_me)
        {
            Sound sound = Sound.Get (loop_me);
            if (_AnimationCurrent != null) {
                if(_AnimationCurrent.Sound != null)
                    AnimationCurrent.Sound.Stop();
            }
            var animation = new SpriteAnimation(Texture, (int)(1000.0 * sound.Duration), true, false, Texture.DefaultRegionIndex);
            animation.Sound = sound;
            PlayAnimation(animation);
        }
    }
}

