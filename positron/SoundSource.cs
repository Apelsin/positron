using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Positron
{
    public class SoundSource : Attachment
    {
        protected Sound _Sound;
        public Sound Sound { get { return _Sound; } set { _Sound = value; }}
        public SoundSource()
        {
        }
        /// <summary>
        /// Play the current assigned sound
        /// </summary>
        /// <param name="follow">SoundSource follows its attached object</param>
        public void Play(bool follow = true)
        {

        }
        public abstract ThreadedRendering Window { get; }
        public abstract PositronGame Game { get; }
        public abstract Scene Scene { get; }
        public abstract RenderSet RenderSet { get; }
        public abstract FarseerPhysics.Dynamics.Body Body { get; }
        public abstract Drawable Drawable { get; }
        public abstract SpriteBase Sprite { get; }
    }
}
