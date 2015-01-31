using System;
using OpenTK;
using FarseerPhysics.Dynamics;

namespace Positron
{
    public delegate void DerezEventHandler(object sender, EventArgs e);
    public interface IWorldObject
    {
        event DerezEventHandler DerezEvent;
        void Update(float time);

        /// <summary>
        /// Connects the Body object with this IWorldObject by
        /// setting the body's user data variable to this object
        /// </summary>
        void InitBlueprints();
        void Derez();

        Body Body { get; }

        //float PositionX { get; set; }
        //float PositionY { get; set; }
        //float PositionZ { get; set; }
        //Vector3 Position { get; set; }
        
        //float ScaleX { get; set; }
        //float ScaleY { get; set; }
        //float ScaleZ { get; set; }
        //Vector3 Scale { get; set; }
        
        //float VelocityX { get; set; }
        //float VelocityY { get; set; }
        //float VelocityZ { get; set; }
        //Vector3 Velocity { get; set; }


        //float Theta { get; set; }
        //quaternion Rotation { get; set; }
    }
}

