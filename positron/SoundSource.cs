using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Positron
{
    public class SoundSource : Extension, IDisposable
    {
        protected static HashSet<SoundSource> Sources = new HashSet<SoundSource>();

        protected int SourceId;

        protected GameObject _Target;
        public GameObject Target { get { return _Target; } set { _Target = value; } }

        protected Sound _Sound;
        public Sound Sound { get { return _Sound; } set { _Sound = value; }}

        public SoundSource(GameObject game_object) : base(game_object)
        {
            SourceId = AL.GenSource();
            Sources.Add(this);
        }
        /// <summary>
        /// Play the current assigned sound
        /// </summary>
        /// <param name="follow">SoundSource follows its attached object</param>
        public void Play(bool follow = true)
        {
            // TODO: follow
            AL.SourcePlay(SourceId);
        }
        public void Stop()
        {
            AL.SourceStop(SourceId);
        }

        public void Update()
        {
            //AL.Source(SourceId, ALSource3f, _Target);
        }

        public void Dispose()
        {
            Stop();
            AL.DeleteSource(SourceId);
            Sources.Remove(this);
        }
        public static void Teardown()
        {
            foreach(SoundSource source in Sources)
            {
                source.Dispose();
            }
        }
    }
}
