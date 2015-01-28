using System;

namespace positron
{
	public class UIButton : SpriteBase, IUIElement, IActuator
	{
		public event RefreshHandler Refresh;
		public event ActionEventHandler Action;
		protected UIElementGroup _Group;
		public UIElementGroup Group { get { return _Group; } }
		protected SpriteBase.SpriteAnimation _ToFree, _ToSelected;
		public UIButton (Scene scene, float x, float y, SpriteBase.SpriteFrame free, SpriteBase.SpriteFrame selected, UIElementGroup grp):
			base(scene.HUD, x, y, free.Texture)
		{
			_Group = grp;
			_Group.Add(this);
			_ToFree = new SpriteAnimation(false, false, free);
			_ToSelected = new SpriteAnimation(false, false, selected);
			Refresh += (sender, e) => {
				var animation = this == _Group.ElementFocused ? _ToSelected : _ToFree;
				_AnimationFrameIndex = _AnimationCurrent == animation ? _AnimationFrameIndex : animation.FrameCount - _AnimationFrameIndex - 1;
				_AnimationCurrent = animation;
			};
			OnRefresh(this, new EventArgs());
		}
		public UIButton (Scene scene, float x, float y, SpriteBase.SpriteFrame free, SpriteBase.SpriteFrame selected):
			this(scene, x, y, free, selected, new UIElementGroup())
		{
		}
		public UIButton (Scene scene, float x, float y, SpriteBase.SpriteFrame free):
			this(scene, x, y, free, free)
		{
		}
		public void OnAction(object sender, ActionEventArgs e)
		{
			if(Action != null)
				Action(sender, e);
		}
		public void OnRefresh(object sender, EventArgs e)
		{
			Refresh(sender, e);
		}
        public override void Dispose()
        {
            this.Action = null;
            this.Refresh = null;
            base.Dispose();
        }
	}
}

