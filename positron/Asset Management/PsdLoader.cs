using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using PhotoshopFile;
using OpenTK;
using Positron.Utility;

namespace Positron
{
    public static class PsdLoader
    {
        public static Texture LoadSpriteSheet(this PositronGame game, string title, params string[] path_components)
        {
            string[] all_path_components = new string[path_components.Length + 1];
            all_path_components[0] = game.Configuration.ArtworkPathFull;
            path_components.CopyTo(all_path_components, 1);
            string path = Path.Combine(all_path_components);
            return LoadSpriteSheetAbsolute(title, path);
        }
        private static Texture LoadSpriteSheetAbsolute (string title, string file_path)
        {
            // Load and decompress Photoshop file structures
            var psdFile = new PsdFile ();
            psdFile.Load (file_path, Encoding.Default);
        
            if (psdFile.Layers.Count == 0) {
                psdFile.BaseLayer.CreateMissingChannels ();
            }
            var layer = psdFile.BaseLayer;
            using (Bitmap bmp = new Bitmap(psdFile.ColumnCount, psdFile.RowCount)) {
                using (Graphics gfx = Graphics.FromImage(bmp)) {
                    gfx.Clear (Color.Transparent);
                    var bmp_data = bmp.LockBits (layer.Rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    int pixel_size = 4 * sizeof(byte);
                    int idx = 0;
                    for (int i = 0; i < bmp_data.Height; i++) {
                        for (int j = 0; j < bmp_data.Width; j++) {
                            Marshal.Copy (new byte[]{
                            layer.Channels [2].ImageData [idx],
                            layer.Channels [1].ImageData [idx],
                            layer.Channels [0].ImageData [idx],
                            layer.Channels [3].ImageData [idx],
                        }, 0, new IntPtr ((long)bmp_data.Scan0 + i * bmp_data.Stride + j * pixel_size), pixel_size);
                            idx++;
                        }
                    }
                    bmp.UnlockBits (bmp_data);
                }
                var texture = Texture.LoadTexture(title, bmp, file_path);
                psdFile.SlicesToTextureRegionInfo (ref texture);
                return texture;
            }
        }
        /// <summary>
        /// Welcome to hell.
        /// </summary>
        /// <param name="psd"></param>
        /// <param name="texture"></param>
        public static void SlicesToTextureRegionInfo (this PsdFile psd, ref Texture texture)
        {
            RawImageResource slices_resource = (RawImageResource)psd.ImageResources.Find (resource => resource.ID == ResourceID.Slices);
            PsdSlicesHeader psd_slices_header = new PsdSlicesHeader ();
            PsdSlice[] psd_slices;

            #region Slices Data Parsing
            // Read the slices data as a stream
            // FixedEndianness and ReadPascalString are defined in Utility.Structure
            using(MemoryStream mem_stream = new MemoryStream(slices_resource.Data))
            {
                using (BinaryReader reader = new BinaryReader(mem_stream, Encoding.BigEndianUnicode))
                {
                    psd_slices_header.Version = reader.ReadInt32 ().FixEndianness ();
                    psd_slices_header.Top = reader.ReadInt32 ().FixEndianness ();
                    psd_slices_header.Left = reader.ReadInt32 ().FixEndianness ();
                    psd_slices_header.Bottom = reader.ReadInt32 ().FixEndianness ();
                    psd_slices_header.Right = reader.ReadInt32 ().FixEndianness ();
                    psd_slices_header.GroupName = reader.ReadPascalString ();
                    psd_slices_header.SliceCount = reader.ReadInt32 ().FixEndianness ();

                    int len = psd_slices_header.SliceCount;
                    psd_slices = new PsdSlice[len];

                    // This is terrifying
                    for (int i = 0; i < len; i++) {
                        psd_slices [i] = new PsdSlice ();
                        psd_slices [i].Id = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].GroupId = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Origin = reader.ReadInt32 ().FixEndianness ();
                        if (psd_slices [i].Origin == 1)
                            psd_slices [i].AssociatedLayerId = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Name = reader.ReadPascalString ();
                        psd_slices [i].Type = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Left = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Top = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Right = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].Bottom = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].URL = reader.ReadPascalString ();
                        psd_slices [i].Target = reader.ReadPascalString ();
                        psd_slices [i].Message = reader.ReadPascalString ();
                        psd_slices [i].AltTag = reader.ReadPascalString ();
                        psd_slices [i].CellIsHTML = reader.ReadBoolean ();
                        psd_slices [i].CellText = reader.ReadPascalString ();
                        psd_slices [i].HorizontalAlignment = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].VerticalAlignment = reader.ReadInt32 ().FixEndianness ();
                        psd_slices [i].A = reader.ReadByte ();
                        psd_slices [i].R = reader.ReadByte ();
                        psd_slices [i].G = reader.ReadByte ();
                        psd_slices [i].B = reader.ReadByte ();
                    }
                    reader.Dispose();
                }
                mem_stream.Dispose();
            }
            #endregion
            var regions = new List<Texture.Region> ();
            for (int i = 0; i < psd_slices.Length; i++) {
                PsdSlice slice = psd_slices [i];
                if (slice.Name != "") {
                    Texture.Region region =
                        new Texture.Region(slice.Name,
                                          new Vector2 (slice.Left, psd.RowCount - slice.Bottom),
                                          new Vector2 (slice.Right, psd.RowCount - slice.Top)); // Vertical axis (Y) is flipped

                    if (slice.Target.ToLower () == "default")
                        texture.DefaultRegionIndex = regions.Count;
                    regions.Add (region);
                }
            }
            texture.Regions = regions.ToArray();
        }
    }
}

