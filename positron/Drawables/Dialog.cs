using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using FarseerPhysics.Dynamics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class DialogSpeaker
	{
		public string Name;
		public Texture Picture;
		public DialogSpeaker ()
		{
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
	public class Dialog : Drawable, IInputAccepter
	{
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
		}
		public override void Render (double time)
		{

			GL.Color4(0, 0, 0, 0.5);
			GL.PushMatrix();
			{
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
			Program.MainGame.SetInputAccepters(ToString(), this);
		}
		public void Next ()
		{
			if (Stanzas == null || StanzaIndex >= Stanzas.Count - 1) {
				End ();
			} else {
				StanzaIndex++;
			}
		}
		public void End()
		{

			Program.MainGame.RemoveInputAccepter(ToString());
			_Shown = false;
			_RenderSet.Remove(this);
			Program.MainGame.TimeStepCoefficient = _RevertTime;
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

