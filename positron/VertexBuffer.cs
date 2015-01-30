using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public struct VertexLite
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public VertexLite(Vector3 p, Vector2 tc)
        {
            Position = p;
            TexCoord = tc;
        }
        public VertexLite(Vector3 p, Vector2 tc, Vector4 c)
        {
            Position = p;
            TexCoord = tc;
        }
        public VertexLite(float px, float py, float pz, float tcx, float tcy)
        {
            Position = new Vector3(px, py, pz);
            TexCoord = new Vector2(tcx, tcy);
        }
        public VertexLite(float px, float py, float pz, float tcx, float tcy, float r, float b, float g, float a)
        {
            Position = new Vector3(px, py, pz);
            TexCoord = new Vector2(tcx, tcy);
        }
        public static readonly int Stride = Marshal.SizeOf(typeof(VertexLite));
    }
    public struct Vertex
    {
        public Vector3 Position;
        //public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector4 Color;
        public Vertex(Vector3 p, /*Vector3 n,*/ Vector2 tc)
        {
            Position = p;
            //Normal = n;
            TexCoord = tc;
            Color = Vector4.One;
        }
        public Vertex(Vector3 p, /*Vector3 n,*/ Vector2 tc, Vector4 c)
        {
            Position = p;
            //Normal = n;
            TexCoord = tc;
            Color = c;
        }
        public Vertex(float px, float py, float pz, /*float nx, float ny, float nz,*/ float tcx, float tcy)
        {
            Position = new Vector3(px, py, pz);
            //Normal = new Vector3(nx, ny, nz);
            TexCoord = new Vector2(tcx, tcy);
            Color = Vector4.One;
        }
        public Vertex(float px, float py, float pz, /*float nx, float ny, float nz,*/ float tcx, float tcy, float r, float b, float g, float a)
        {
            Position = new Vector3(px, py, pz);
            //Normal = new Vector3(nx, ny, nz);
            TexCoord = new Vector2(tcx, tcy);
            Color = new Vector4(r, g, b, a);
        }
        public static readonly int Stride = Marshal.SizeOf(typeof(Vertex));
    }
    public class VertexBuffer : IDisposable
    {
        private static readonly IntPtr VertexPtr = new IntPtr(0);
        //private static IntPtr NormalPtr = new IntPtr(Vector3.SizeInBytes + VertexPtr.ToInt64());
        private static readonly IntPtr TexCoordPtr = new IntPtr(Vector3.SizeInBytes + VertexPtr.ToInt64());
        private static readonly IntPtr ColorPtr = new IntPtr(Vector2.SizeInBytes + TexCoordPtr.ToInt64());
        

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
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
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
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
        }

        public void Render ()
        {
            GL.BindBuffer (BufferTarget.ArrayBuffer, Id);
            GL.VertexPointer (3, VertexPointerType.Float, _Size, VertexPtr);
            //GL.NormalPointer(NormalPointerType.float, _Size, NormalPtr);
            GL.TexCoordPointer (2, TexCoordPointerType.Float, _Size, TexCoordPtr);
            //GL.ColorPointer (4, ColorPointerType.Float, _Size, ColorPtr);
            if (_ColorBufferUsed) {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer (4, ColorPointerType.Float, _Size, ColorPtr);
            }
            else
                GL.DisableClientState(ArrayCap.ColorArray);
            GL.DrawArrays(PrimitiveType.Quads, 0, _DataLength); // TODO: Make this part not-hardcoded
        }
		public void Dispose()
		{
			GL.DeleteBuffer(_Id);
		}
    }
}
