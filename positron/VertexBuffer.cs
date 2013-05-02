using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public struct VertexLite
    {
        public Vector3d Position;
        public Vector2d TexCoord;
        public VertexLite(Vector3d p, Vector2d tc)
        {
            Position = p;
            TexCoord = tc;
        }
        public VertexLite(Vector3d p, Vector2d tc, Vector4 c)
        {
            Position = p;
            TexCoord = tc;
        }
        public VertexLite(double px, double py, double pz, double tcx, double tcy)
        {
            Position = new Vector3d(px, py, pz);
            TexCoord = new Vector2d(tcx, tcy);
        }
        public VertexLite(double px, double py, double pz, double tcx, double tcy, float r, float b, float g, float a)
        {
            Position = new Vector3d(px, py, pz);
            TexCoord = new Vector2d(tcx, tcy);
        }
        public static readonly int Stride = Marshal.SizeOf(default(VertexLite));
    }
    public struct Vertex
    {
        public Vector3d Position;
        //public Vector3d Normal;
        public Vector2d TexCoord;
        public Vector4 Color;
        public Vertex(Vector3d p, /*Vector3d n,*/ Vector2d tc)
        {
            Position = p;
            //Normal = n;
            TexCoord = tc;
            Color = Vector4.One;
        }
        public Vertex(Vector3d p, /*Vector3d n,*/ Vector2d tc, Vector4 c)
        {
            Position = p;
            //Normal = n;
            TexCoord = tc;
            Color = c;
        }
        public Vertex(double px, double py, double pz, /*double nx, double ny, double nz,*/ double tcx, double tcy)
        {
            Position = new Vector3d(px, py, pz);
            //Normal = new Vector3d(nx, ny, nz);
            TexCoord = new Vector2d(tcx, tcy);
            Color = Vector4.One;
        }
        public Vertex(double px, double py, double pz, /*double nx, double ny, double nz,*/ double tcx, double tcy, float r, float b, float g, float a)
        {
            Position = new Vector3d(px, py, pz);
            //Normal = new Vector3d(nx, ny, nz);
            TexCoord = new Vector2d(tcx, tcy);
            Color = new Vector4(r, g, b, a);
        }
        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }
    public sealed class VertexBuffer : IDisposable // Sealed for performance boost
    {
        private static IntPtr VertexPtr = new IntPtr(0);
        //private static IntPtr NormalPtr = new IntPtr(Vector3d.SizeInBytes + VertexPtr.ToInt64());
        private static IntPtr TexCoordPtr = new IntPtr(Vector3d.SizeInBytes + VertexPtr.ToInt64());
        private static IntPtr ColorPtr = new IntPtr(Vector2d.SizeInBytes + TexCoordPtr.ToInt64());
        

        protected int _Id;
        public int Id
        {
            get
            {
                // Create an id on first use.
                if (_Id == 0)
                {
                    GraphicsContext.Assert();
                    GL.GenBuffers(1, out _Id);
                    if (_Id == 0)
                        throw new Exception("Could not create VBO.");
                }
                return _Id;
            }
        }
        protected int _DataLength;
        public int DataLength { get { return _DataLength; } }

        protected bool _ColorBufferUsed = false;
        public bool ColorBufferUsed { get { return _ColorBufferUsed; } }

        protected int _Size = VertexLite.Stride;

        public VertexBuffer(params Vertex[] data)
        {
            SetData(data);
        }

        public VertexBuffer(params VertexLite[] data)
        {
            SetData(data);
        }

        public void SetData (params Vertex[] data)
        {
            if (data == null)
                throw new ArgumentNullException ("data");

            _DataLength = data.Length;
            _ColorBufferUsed = true;
            _Size = Vertex.Stride;

            GL.BindBuffer (BufferTarget.ArrayBuffer, Id);
            GL.BufferData (BufferTarget.ArrayBuffer, new IntPtr (data.Length * _Size), data, BufferUsageHint.StaticDraw);
        }

        public void SetData (params VertexLite[] data)
        {
            if (data == null)
                throw new ArgumentNullException ("data");
            
            _DataLength = data.Length;
            _ColorBufferUsed = false;
            _Size = VertexLite.Stride;
            
            GL.BindBuffer (BufferTarget.ArrayBuffer, Id);
            GL.BufferData (BufferTarget.ArrayBuffer, new IntPtr (data.Length * _Size), data, BufferUsageHint.StaticDraw);
        }

        public void Render ()
        {
            GL.BindBuffer (BufferTarget.ArrayBuffer, Id);
            GL.VertexPointer (3, VertexPointerType.Double, _Size, VertexPtr);
            //GL.NormalPointer(NormalPointerType.Double, _Size, NormalPtr);
            GL.TexCoordPointer (2, TexCoordPointerType.Double, _Size, TexCoordPtr);
            //GL.ColorPointer (4, ColorPointerType.Float, _Size, ColorPtr);
            if (_ColorBufferUsed) {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer (4, ColorPointerType.Float, _Size, ColorPtr);
            }
            else
                GL.DisableClientState(ArrayCap.ColorArray);
            GL.DrawArrays(BeginMode.Quads, 0, _DataLength); // TODO: Make this part not-hardcoded
        }
		public void Dispose()
		{
			GL.DeleteBuffer(_Id);
		}
    }
}
