using System;
using System.Collections;
using System.IO;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Positron
{
    public class Sound
    {
        private static IntPtr AudioDevice;
        private static ContextHandle AudioCtx;
        //private static XRamExtension XRam = new XRamExtension();
        private static Hashtable Sounds = new Hashtable();

        protected int BufferId;
        protected int SourceId;
        protected float _Duration;
        public float Duration { get { return _Duration; } }

        protected Sound (int buffer, int source, float duration)
        {
            BufferId = buffer;
            SourceId = source;
            _Duration = duration;
        }
        public static void InitialSetup ()
        {
            // FOR THE LOVE OF GOD PLEASE LET THIS WORK
            AudioDevice = Alc.OpenDevice (null); // Default device
            if (AudioDevice != null) {
                AudioCtx = Alc.CreateContext(AudioDevice, (int[])null);
                if(AudioCtx != ContextHandle.Zero)
                {
                    Alc.GetError(AudioDevice);
                    if(Alc.MakeContextCurrent( AudioCtx ))
                    {
                        //LoadWaveFile ("camprespite_loop", "camprespite_loop.wav");
                        LoadWaveFile ("last_human_loop", "last_human_loop_limited.wav");//.DurationAdjust(0.01);
                        LoadWaveFile ("induction_loop", "induction_loop.wav");
                        LoadWaveFile ("sfx_bullet_impact", "sfx_bullet_impact.wav");
                        LoadWaveFile ("sfx_player_land_two_feet", "sfx_player_land_two_feet.wav");
                        LoadWaveFile ("sfx_shoot_gun", "sfx_shoot_gun.wav");
                        LoadWaveFile ("win", "win.wav");
                    }
                    else
                        throw new Exception("Failed to set current audio context");
                }
                else
                    throw new Exception("Failed to create audio context.");
            }
            else
                throw new Exception("Failed to open default audio device.");
        }
        public static void Teardown ()
        {
            foreach (object o in Sounds.Keys) {
                Sound sound = (Sound)Sounds[o];
                AL.SourceStop(sound.SourceId);
                AL.DeleteSource(sound.SourceId);
                AL.DeleteBuffer(sound.BufferId);
            }
            if(Alc.MakeContextCurrent( ContextHandle.Zero ))
            {
                Alc.DestroyContext( AudioCtx );
                if(Alc.CloseDevice( AudioDevice ))
                {
                }
            }
        }
        public static void KillTheNoise ()
        {
            foreach (object o in Sounds.Keys) {
                Sound sound = (Sound)Sounds [o];
                AL.SourceStop (sound.SourceId);
            }
        }
        public static Sound Get (object key)
        {
            if(Sounds.ContainsKey(key))
                return (Sound)Sounds[key];
            return null;
        }
        public void DurationAdjust (float dt)
        {
            _Duration += dt;
        }
        public void Play ()
        {
            AL.SourcePlay(SourceId);
        }
        public void Stop ()
        {
            AL.SourceStop(SourceId);
        }
        public static void Play (object key)
        {
            Get (key).Play();
        }
        public static Sound LoadWaveFile(string title, params string[] path_components)
        {
            string[] all_path_components = new string[path_components.Length + 1];
            all_path_components[0] = Configuration.AudioPath;
            path_components.CopyTo(all_path_components, 1);
            string path = Path.Combine(all_path_components);
            return LoadWaveFileAbsolute(title, path);
        }
        protected static Sound LoadWaveFileAbsolute(string title, string file_path)
        {   
            if(!System.IO.File.Exists(file_path))
                throw new FileNotFoundException("File missing: ", file_path);
            int buffer_id = AL.GenBuffer();
            int source_id = AL.GenSource();
//            if(XRam.IsInitialized)
//                XRam.SetBufferMode(1, ref buffer_id, XRamExtension.XRamStorage.Hardware);

            int channels, bits_per_sample, sample_rate;
            byte[] sound_data = LoadWave(File.Open(file_path, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
            AL.BufferData(buffer_id, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);
            AL.Source(source_id, ALSourcei.Buffer, buffer_id);
            float duration = (float)sound_data.Length / (float)(sample_rate * channels * bits_per_sample / 8);
            return (Sound)(Sounds[title] = new Sound(buffer_id, source_id, duration));
        }

        // Loads a wave/riff audio file.
        protected static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");
                
                int riff_chunck_size = reader.ReadInt32();
                
                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");
                
                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");
                
                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();
                
                string data_signature = new string(reader.ReadChars(4));
                if(data_signature == "LIST")
                {
                    int length = reader.ReadInt32();
                    reader.ReadBytes(length);
                    data_signature = new string(reader.ReadChars(4));
                }

//                if (data_signature != "data")
//                    throw new NotSupportedException("Specified wave file is not supported.");
                
                int data_chunk_size = reader.ReadInt32();
                
                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;
                
                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}

