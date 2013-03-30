using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public struct Vertex
    {
        public Vector3d Position, Normal;
        public Vector2d TexCoord;
        public Vertex(Vector3d p, Vector3d n, Vector2d tc)
        {
            Position = p;
            Normal = n;
            TexCoord = tc;
        }
        public Vertex(double px, double py, double pz, double nx, double ny, double nz, double tcx, double tcy)
        {
            Position = new Vector3d(px, py, pz);
            Normal = new Vector3d(nx, ny, nz);
            TexCoord = new Vector2d(tcx, tcy);
        }
        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }
    public sealed class VertexBuffer // Sealed for performance boost
    {
        private int _Id;
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
        private int _DataLength;
        public int DataLength { get { return _DataLength; } }

        public VertexBuffer()
        {
        }

        public VertexBuffer(params Vertex[] data)
        {
            SetData(data);
        }

        public void SetData(params Vertex[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(data.Length * Vertex.Stride), data, BufferUsageHint.StaticDraw);
            _DataLength = data.Length;
        }

        public void Render()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.VertexPointer(3, VertexPointerType.Double, Vertex.Stride, new IntPtr(0));
            GL.NormalPointer(NormalPointerType.Double, Vertex.Stride, new IntPtr(Vector3d.SizeInBytes));
            GL.TexCoordPointer(2, TexCoordPointerType.Double, Vertex.Stride, new IntPtr(2 * Vector3d.SizeInBytes));
            GL.DrawArrays(BeginMode.Quads, 0, _DataLength); // TODO: Make this part not-hardcoded
        }
    }
}
