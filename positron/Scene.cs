using System;
using System.Collections;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;

using OpenTK;

namespace positron
{
	public class Scene
	{
		#region State
		#region Member Variables
		private string Name;
		private Vector2d _SceneSize;
		private World _World;
		/// <summary>
		/// All of the render sets
		/// </summary>
		public RenderSet All;
		/// <summary>
		/// The background set is the farthest away from the viewport and is drawn first
		/// </summary>
		public RenderSet Background;
		/// <summary>
		/// The rear set is drawn atop the background and should contain distant objects
		/// </summary>
		public RenderSet Rear;
		/// <summary>
		/// The stage set contains the focal elements of the scene (gameplay critical)
		/// </summary>
		public RenderSet Stage;
		/// <summary>
		/// Tests render set
		/// </summary>
		public RenderSet Tests;
		/// <summary>
		/// The front set is in front of the stage to draw forground objects
		/// </summary>
		public RenderSet Front;
		/// <summary>
		/// The HUD set is the foremost render set and will draw the game's GUI
		/// </summary>
		public RenderSet HUD;
		#endregion
		#region Static Variables
		private static Hashtable Scenes = new Hashtable();
		#endregion
		#region Member Accessors
		public Vector2d SceneSize { get { return _SceneSize; } }
		public World World { get { return _World; } }
		#endregion
		#region Static Accessors
		#endregion
		#endregion
		#region Behavior
		#region Member
		private Scene (string name)
		{
			Background = new RenderSet();
			Rear = new RenderSet();
			Stage = new RenderSet();
			Tests = new RenderSet();
			Front = new RenderSet();
			HUD = new RenderSet();

			// This should contain everything AllRednerSetsInOrder would contain
			// This is an optimization over using an enumerable
			All = new RenderSet(Background, Rear, Stage, Tests, Front, HUD);

			// This is a little irritating but I'll just have to deal with it for now:
			Microsoft.Xna.Framework.Vector2 gravity =
				new Microsoft.Xna.Framework.Vector2(0.0f, (float)Configuration.ForceDueToGravity);

			_World = new World(gravity);
		}
//		public bool CheckIn(object o, )
//		{
//		}
		public static Scene Create (string name)
		{
			Scene scene = new Scene(name);
			Scenes.Add(name, scene);
			return scene;
		}
		public void Update (double time)
		{
			// Update the world!
			World.Step((float)time);
		}
		public void Render (double time)
		{
			All.Render((float)time);
		}
		public IEnumerable<RenderSet> AllRenderSetsInOrder()
		{
			yield return Background;
			yield return Rear;
			yield return Stage;
			yield return Tests;
			yield return Front;
			yield return HUD;
		}
		/// <summary>
		/// Includes preserved <see cref="IWorldObject"/>World Object</summary>s from previous scene
		/// </summary>
		private void PreservationTransfer (Scene previous_scene)
		{
			// Get the enumerable for the render sets; these need to be aligned
			IEnumerable<RenderSet> previous_sets = previous_scene.AllRenderSetsInOrder();
			IEnumerable<RenderSet> these_sets = AllRenderSetsInOrder();
			IEnumerator<RenderSet> this_set_enum = these_sets.GetEnumerator();
			foreach (RenderSet r in previous_sets)				// For each render set
			{
				foreach(RenderSet r__r in r)					// For each render set in render set
				{
					if(r__r is IWorldObject)					// If renderable is world object
					{
						IWorldObject w = (IWorldObject)r__r;	// Cast to world object
						if(w.Preserve) 							// If world object is preserved
						{
							this_set_enum.Current.Add(r__r);	// Add in this renderable/world object
							SceneChangeEventArgs scea = new SceneChangeEventArgs();
							scea.From = previous_scene;
							scea.To = this;
							w.SceneChange(null, scea);
						}
					}
				}
				if(!this_set_enum.MoveNext())					// Advance enumerator; if passed end...
					break;										// ...get the hell out of Dodge
			}
		}
		#endregion
		#region Static
		#endregion
		#endregion
	}
	public class SceneChangeEventArgs : EventArgs
	{
		public Scene From { get; set; }
		public Scene To { get; set; }
	}
}

