using System;
using OpenTK;
using FarseerPhysics.Dynamics;

namespace positron
{
	public interface IWorldObject : ISceneElement
	{
		void Update(double time);

		/// <summary>
		/// Connects the Body object with this IWorldObject by
		/// setting the body's user data variable to this object
		/// </summary>
		void InitBlueprints();
		void Derez();

		Body Body { get; set; }

		double PositionX { get; set; }
		double PositionY { get; set; }
		//double PositionZ { get; set; }
		Vector3d Position { get; set; }
		
		double ScaleX { get; set; }
		double ScaleY { get; set; }
		//double ScaleZ { get; set; }
		Vector3d Scale { get; set; }
		
		double VelocityX { get; set; }
		double VelocityY { get; set; }
		//double VelocityZ { get; set; }
		Vector3d Velocity { get; set; }


		double Theta { get; set; }
		//quaternion Rotation { get; set; }
	}
}

