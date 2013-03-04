using System;
using OpenTK;

namespace positron
{
	public interface IWorldObject
	{
		double PositionX { get; set; }
		double PositionY { get; set; }
		//double PositionZ { get; set; }
		Vector3d Position { get; set; }
		
		double SizeX { get; set; }
		double SizeY { get; set; }
		//double SizeZ { get; set; }
		Vector3d Size { get; set; }
		
		double VelocityX { get; set; }
		double VelocityY { get; set; }
		//double VelocityZ { get; set; }
		Vector3d Velocity { get; set; }


		double Theta { get; set; }
		//quaternion Rotation { get; set; }
	}
}

