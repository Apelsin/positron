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
		protected RenderSet _RenderSet;
		protected List<DialogStanza> Stanzas;
		protected int StanzaIndex = 0;
		protected float _PauseTime = 0.0f;
		protected float _RevertTime = 1.0f;
		protected Texture	BackTL,	BackT,	BackTR,
							BackL,	BackC,	BackR,
							BackBL, BackB, 	BackBR;
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
			SizeX = _RenderSet.Scene.ViewSize.X;
			SizeY = (int)(_RenderSet.Scene.ViewSize.Y * 0.25);
			// TODO: use one texture and use texture mapping
			BackTL = Texture.Get ("sprite_dialog_box_tl");
			BackT = Texture.Get ("sprite_dialog_box_t");
			BackTR = Texture.Get ("sprite_dialog_box_tr");
			BackL = Texture.Get ("sprite_dialog_box_l");
			BackC = Texture.Get ("sprite_dialog_box_c");
			BackR = Texture.Get ("sprite_dialog_box_r");
			BackBL = Texture.Get ("sprite_dialog_box_bl");
			BackB = Texture.Get ("sprite_dialog_box_b");
			BackBR = Texture.Get ("sprite_dialog_box_br");
		}
		public override void Render (double time)
		{
			Texture t;
			double sx = Math.Max (1.0, SizeX - BackL.Width - BackR.Width);
			double sy = Math.Max (1.0, SizeY - BackT.Height - BackB.Width);
			GL.Color4(Color.White);
			GL.PushMatrix();
			{
				// So much for DRY...
				GL.Translate (_Position.X, _Position.Y + SizeY - BackT.Height, _Position.Z);
				GL.PushMatrix();
				{
					/**********************/		(t = BackTL).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();
					GL.Translate(t.Width, 0, 0);	(t = BackT).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(sx,		0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(sx,		t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();
					GL.Translate(sx, 0, 0);	(t = BackTR).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();
				}
				GL.PopMatrix();
				GL.Translate (0.0, -sy, 0.0);
				GL.PushMatrix();
				{
					/**********************/		(t = BackL).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, sy		);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	sy		);
					GL.End ();
					GL.Translate(t.Width, 0, 0);	(t = BackC).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(sx,		0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(sx, 		sy		);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	sy		);
					GL.End ();
					GL.Translate(sx, 0, 0);	(t = BackR).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, sy		);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	sy		);
					GL.End ();
				}
				GL.PopMatrix();
				GL.Translate (0.0, -t.Height, 0.0);
				GL.PushMatrix();
				{
					/**********************/		(t = BackBL).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();

					GL.Translate(t.Width, 0, 0);	(t = BackB).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(sx,		0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(sx,		t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();

					GL.Translate(sx, 0, 0);	(t = BackBR).Bind ();
					GL.Begin (BeginMode.Quads);
					GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
					GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
					GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, t.Height);
					GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
					GL.End ();
				}
				GL.PopMatrix();
			}
			GL.PopMatrix();
		}
		public override double RenderSizeX()
		{
			return SizeX;
		}
		public override double RenderSizeY()
		{
			return SizeY;
		}
		public override string ToString ()
		{
			return string.Format ("[Dialog: Title={0}, Scene={1}]", _Title, _RenderSet);
		}
		public void Begin()
		{
			_RevertTime = Program.Game.TimeStepCoefficient;
			Program.Game.TimeStepCoefficient = _PauseTime;
			_RenderSet.Add(this);
			_Shown = true;
			StanzaIndex = 0;
			Program.Game.SetInputAccepters(ToString(), this);
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

			Program.Game.RemoveInputAccepter(ToString());
			_Shown = false;
			_RenderSet.Remove(this);
			Program.Game.TimeStepCoefficient = _RevertTime;
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

