using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using FarseerPhysics.Dynamics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class DialogSpeaker
	{
		public string Name;
		public Texture Picture;
		protected static Hashtable DialogSpeakers = new Hashtable();
		public DialogSpeaker (string name, Texture picture)
		{
			Name = name;
			Picture = picture;
		}
		public static void InitialSetup()
		{
			DialogSpeakers.Add ("protagonist", new DialogSpeaker("Volta", Texture.Get ("sprite_protagonist_picture")));
		}
		public static DialogSpeaker Get (object key)
		{
			return (DialogSpeaker)DialogSpeakers[key];
		}
	}
	public class DialogStanza
	{
		public DialogSpeaker Speaker;
		public string Message;
		public DialogStanza(DialogSpeaker speaker, string message)
		{
			Speaker = speaker;
			Message = message;
		}
	}
	public class DialogEndEventArgs : EventArgs
	{
	}
	public delegate void DialogEndEventHandler(object sender, DialogEndEventArgs e);
	public class Dialog : Drawable, IInputAccepter
	{
		public event DialogEndEventHandler DialogEnd;
		protected PTextWriter SpeakerWriter;
		protected PTextWriter SpeechWriter;
		protected string _Title;
		protected bool _Shown;
		protected List<DialogStanza> Stanzas;
		protected int StanzaIndex = 0;
		protected float _PauseTime = 0.0f;
		protected float _RevertTime = 1.0f;
		protected Texture FadeUp;
		public string Title { get { return _Title; } }
		public DialogStanza CurrentStanza {
			get { return Stanzas [StanzaIndex]; }
		}
		public float PauseTime {
			get { return _PauseTime; }
			set { _PauseTime = value; }
		}
		public float RevertTime {
			get { return _RevertTime; }
		}
		public bool Shown { get { return _Shown; } }
		public Dialog(RenderSet render_set, string title, List<DialogStanza> stanzas):
			base(null)
		{
			_Title = title;
			Stanzas = stanzas;
			_RenderSet = render_set;
			_Shown = false;
			ScaleX = _RenderSet.Scene.ViewSize.X;
			ScaleY = (int)(_RenderSet.Scene.ViewSize.Y * 0.25);
			FadeUp = Texture.Get("sprite_dialog_fade_up");

			SpeakerWriter = new PTextWriter(new Size((int)ScaleX, 24));
			SpeechWriter = new PTextWriter(new Size((int)ScaleX, (int)ScaleY - 24));
		}
		public override void Render (double time)
		{
			GL.PushMatrix();
			{
				GL.Color4(0, 0, 0, 0.5);
				GL.BindTexture(TextureTarget.Texture2D, 0);
				GL.Translate (_Position.X, _Position.Y, 0.0);
				GL.Begin (BeginMode.Quads);
				GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
				GL.TexCoord2(1.0,  0.0);		GL.Vertex2(ScaleX,	0.0		);
				GL.TexCoord2(1.0, -1.0);		GL.Vertex2(ScaleX,	ScaleY);
				GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	ScaleY);
				GL.End ();
				GL.Translate (0.0, ScaleY, 0.0);
				Texture.Bind(FadeUp);
				GL.Begin (BeginMode.Quads);
				GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
				GL.TexCoord2(1.0,  0.0);		GL.Vertex2(ScaleX,	0.0		);
				GL.TexCoord2(1.0, -1.0);		GL.Vertex2(ScaleX,	FadeUp.Height);
				GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	FadeUp.Height);
				GL.End ();


			}
			GL.PopMatrix();
			GL.Color4(0.5, 0.5, 0.75, 1.0);
			GL.PushMatrix();
			{
				SpeakerWriter.Render(time);
				GL.Translate (0.0, 24, 0.0);
				if(CurrentStanza.Speaker != null)
				{
					var picture = CurrentStanza.Speaker.Picture;
					if(picture != null)
					{
						picture.Bind();
						GL.Begin (BeginMode.Quads);
						GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 			0.0);
						GL.TexCoord2(1.0,  0.0);		GL.Vertex2(picture.Width,	0.0);
						GL.TexCoord2(1.0, -1.0);		GL.Vertex2(picture.Width,	picture.Height);
						GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 			picture.Height);
						GL.End ();
						GL.Translate(picture.Width + 24, 0.0, 0.0);
					}
				}
				SpeechWriter.Render(time);
			}
			GL.PopMatrix();
		}
		public override string ToString ()
		{
			return string.Format ("[Dialog: Title={0}, Scene={1}]", _Title, _RenderSet);
		}
		public void Begin()
		{
			_RevertTime = Program.MainGame.TimeStepCoefficient;
			Program.MainGame.TimeStepCoefficient = _PauseTime;
			_RenderSet.Add(this);
			_Shown = true;
			StanzaIndex = 0;
			UpdateContent();
			Program.MainGame.SetInputAccepters(ToString(), this);
		}
		public void Next ()
		{
			if (Stanzas == null || StanzaIndex >= Stanzas.Count - 1) {
				End ();
			} else {
				StanzaIndex++;
				UpdateContent();
			}
		}
		protected void UpdateContent ()
		{
			// Update the actual GL Texture with the rendered text in the thread
			// with the GL context
			Program.MainGame.AddUpdateEventHandler (this, (sender2, e2) => {
				SpeakerWriter.Clear();
				if(CurrentStanza.Speaker != null)
				{
					string speaker_text = CurrentStanza.Speaker.Name;
					SpeakerWriter.AddLine (speaker_text, PointF.Empty, Brushes.LightBlue);
				}

				string stanza_text = CurrentStanza.Message;
				SpeechWriter.Clear ();
				SpeechWriter.AddLine (stanza_text);
				return true;
			});
		}
		public void End()
		{
			Program.MainGame.RemoveInputAccepters(ToString());
			_Shown = false;
			_RenderSet.Remove(this);
			Program.MainGame.TimeStepCoefficient = _RevertTime;
			if(DialogEnd != null)
				DialogEnd(this, new DialogEndEventArgs());
		}
		public bool KeyDown (object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Space) {
				Next();
				return false;
			}
			return true;
		}
		public bool KeyUp(object sender, KeyboardKeyEventArgs e)
		{
			return true;
		}
		public KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e)
		{
			//e.KeysPressedWhen.Clear();
			return e;
		}
	}
}

