#region License and Information
/*****
* GIFLoader.cs
* -------------
* 
* 2017.04.16 - first version 
* 
* Copyright (c) 2017 Markus Göbel (Bunny83)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to
* deal in the Software without restriction, including without limitation the
* rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
* sell copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* 
*****/
#endregion License and Information
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace B83.Image.GIF
{
    #region Enums and interfaces
    public enum EScreenDescriptorFlags : byte
    {
        GlobalColorTableFlag    = 0x80,  // 1 Bit   1000 0000
        ColorResolution         = 0x70,  // 3 Bits  0111 0000
        SortFlag                = 0x08,  // 1 Bit   0000 1000
        SizeOfGlobalColorTable  = 0x07,  // 3 Bits  0000 0111
    }
    public enum EImageDescriptorFlags : byte
    {
        LocalColorTableFlag     = 0x80,   // 1 Bit   1000 0000
        InterlaceFlag           = 0x40,   // 1 Bit   0100 0000
        SortFlag                = 0x20,   // 1 Bit   0010 0000
        Reserved                = 0x18,   // 2 Bits  0001 1000
        SizeOfLocalColorTable   = 0x07,   // 3 Bits  0000 0111
    }
    public enum EBlockType : byte
    {
        ImageDescriptor = 0x2C,
        Extension = 0x21,
        Trailer = 0x3B,
    }
    public enum EExtensionType : byte
    {
        GraphicControl = 0xF9,
        Comment = 0xFE,
        PlainText = 0x01,
        Application = 0xFF,
    }

    public enum EGraphicControlFlags : byte
    {
        TransparentColorFlag = 0x01, // 0000 0001
        UserInputFlag = 0x02,        // 0000 0010
        DisposalMethod = 0x1C,       // 0001 1100
        _Reserved = 0xE0,            // 1110 0000
    }
    public enum EDisposalMethod : byte
    {
        NoAction = 0,
        DoNotDispose = 1,
        RestoreBackgroundColor = 2,
        RestorePrevious = 3,
    }


    public interface IGIFBlock
    {
        GIFImage Parent { get; set; }
        EBlockType blockType { get; }
    }
    public interface IGIFExtension
    {
        EExtensionType extType { get; }
    }
    public interface IGIFRenderingBlock : IGIFBlock
    {
        GIFGraphicControlExt graphicControl { get; set; }
        void DrawTo(Color32[] aData, int aWidth, int aHeight, int aXOffset = 0, int aYOffset = 0);
        void Dispose(Color32[] aData, int aWidth, int aHeight, int aXOffset = 0, int aYOffset = 0);
    }
    #endregion Enums and interfaces

    #region Header structs
    public struct GIFHeader
    {
        public static uint MAGIC = 0x464947; // GIF
        public static uint VERSION_87a = 0x613738; // 87a
        public static uint VERSION_89a = 0x613938; // 89a
        public uint magic; // 3 byte
        public uint version; // 3 byte
        public string Magic { get { return string.Empty + GetMagic(0) + GetMagic(1) + GetMagic(2); } }
        private char GetMagic(int aIndex) { return (char)((magic >> 8 * aIndex) & 0xFF); }
        public string Version { get { return string.Empty + GetVersion(0) + GetVersion(1) + GetVersion(2); } }
        private char GetVersion(int aIndex) { return (char)((version >> 8 * aIndex) & 0xFF); }
    }


    public struct GIFScreenDescriptor
    {
        public ushort width;
        public ushort height;
        public EScreenDescriptorFlags flags;
        public byte bgColorIndex;
        public byte pixelAspectRatio;
        public Color32[] globalColorTable;
        public bool HasGlobalColorTable
        {
            get { return (flags & EScreenDescriptorFlags.GlobalColorTableFlag) > 0; }
            set { flags = value ? (flags | EScreenDescriptorFlags.GlobalColorTableFlag) : (flags & ~EScreenDescriptorFlags.GlobalColorTableFlag); }
        }
        public bool IsColorTableSorted
        {
            get { return (flags & EScreenDescriptorFlags.SortFlag) > 0; }
            set { flags = value ? (flags | EScreenDescriptorFlags.SortFlag) : (flags & ~EScreenDescriptorFlags.SortFlag); }
        }
        public int ColorResolution
        {
            get { return ((int)(flags & EScreenDescriptorFlags.ColorResolution) >> 4); }
            set { flags = (flags & ~EScreenDescriptorFlags.ColorResolution) | ((EScreenDescriptorFlags)(value << 4) & EScreenDescriptorFlags.ColorResolution); }
        }
        private int _SizeOfGlobalColorTable
        {
            get { return ((int)(flags & EScreenDescriptorFlags.SizeOfGlobalColorTable)); }
            set { flags = (flags & ~EScreenDescriptorFlags.SizeOfGlobalColorTable) | ((EScreenDescriptorFlags)(value) & EScreenDescriptorFlags.SizeOfGlobalColorTable); }
        }
        public int SizeOfGlobalColorTable
        {
            get { return 2 << (_SizeOfGlobalColorTable); }
            set { _SizeOfGlobalColorTable = (int)(Math.Log(value >> 1) / Math.Log(2)); }
        }
    }
    #endregion Header structs

    #region Blocks and extensions
    public class GIFGraphicControlExt : IGIFExtension, IGIFBlock
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.Extension; } }
        public EExtensionType extType { get { return EExtensionType.GraphicControl; } }
        public EGraphicControlFlags flags;
        public ushort delay;
        public byte transparentColorIndex;
        public float fdelay { get { return delay / 100f; } }
        public bool WaitForUserInput { get { return (flags & EGraphicControlFlags.UserInputFlag) > 0; } }
        public bool HasTransparentColorIndex { get { return (flags & EGraphicControlFlags.TransparentColorFlag) > 0; } }
        public EDisposalMethod DisposalMethod
        {
            get { return (EDisposalMethod)((int)(flags & EGraphicControlFlags.DisposalMethod) >> 2); }
            set { flags = (flags & ~EGraphicControlFlags.DisposalMethod) | (EGraphicControlFlags)((int)value << 2) & EGraphicControlFlags.DisposalMethod; }
        }
    }

    public class GIFImageBlock : IGIFRenderingBlock
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.ImageDescriptor; } }
        public GIFGraphicControlExt graphicControl { get; set; }
        public ushort xPos;
        public ushort yPos;
        public ushort width;
        public ushort height;
        public EImageDescriptorFlags flags;
        public Color32[] colorTable;
        public Color32[] usedColorTable;
        public List<byte> data;
        public int packedSize;
        internal int _Interlaced81 = 0; // count of lines of stage 1
        internal int _Interlaced82 = 0; // count of lines of stage 2
        internal int _Interlaced4 = 0;  // count of lines of stage 3
        private int _SizeOfLocalColorTable
        {
            get { return ((int)(flags & EImageDescriptorFlags.SizeOfLocalColorTable)); }
            set { flags = (flags & ~EImageDescriptorFlags.SizeOfLocalColorTable) | ((EImageDescriptorFlags)(value) & EImageDescriptorFlags.SizeOfLocalColorTable); }
        }
        public int SizeOfLocalColorTable
        {
            get { return 2 << (_SizeOfLocalColorTable); }
            set { _SizeOfLocalColorTable = (int)(Math.Log(value >> 1) / Math.Log(2)); }
        }
        public bool HasLocalColorTable
        {
            get { return (flags & EImageDescriptorFlags.LocalColorTableFlag) > 0; }
            set { flags = value ? (flags | EImageDescriptorFlags.LocalColorTableFlag) : (flags & ~EImageDescriptorFlags.LocalColorTableFlag); }
        }
        public bool IsColorTableSorted
        {
            get { return (flags & EImageDescriptorFlags.SortFlag) > 0; }
            set { flags = value ? (flags | EImageDescriptorFlags.SortFlag) : (flags & ~EImageDescriptorFlags.SortFlag); }
        }
        public bool IsInterlaced
        {
            get { return (flags & EImageDescriptorFlags.InterlaceFlag) > 0; }
            set { flags = value ? (flags | EImageDescriptorFlags.InterlaceFlag) : (flags & ~EImageDescriptorFlags.InterlaceFlag); }
        }
        internal void CalcInterlacedLimits()
        {
            int h = height - 1;
            _Interlaced81 = (h >> 3) + 1;
            if (height > 4)
                _Interlaced82 = ((h + 4) >> 3);
            if (height > 2)
                _Interlaced4 = (h + 2) >> 2;
        }
        internal int GetInterlacedIndex(int aIndex)
        {
            // Gotcha: The shift operator(<<) has less priority than addition(+)
            // that's why (aIndex << 3) need to be in brackets.
            if (aIndex < _Interlaced81)
                return aIndex << 3;      // every 8th starting at 0

            aIndex -= _Interlaced81;
            if (aIndex < _Interlaced82)
                return 4 + (aIndex << 3);  // every 8th starting at 4

            aIndex -= _Interlaced82;
            if (aIndex < _Interlaced4)
                return 2 + (aIndex << 2);  // every 4th starting at 2

            aIndex -= _Interlaced4;
            return 1 + (aIndex << 1);      // every 2nd starting at 1

        }
        /* aImageHeight == 16
         *  0 0
         *  1       8
         *  2     4
         *  3       9
         *  4   2
         *  5       10
         *  6     5
         *  7       11
         *  8 1
         *  9       12
         * 10     6
         * 11       13
         * 12   3
         * 13       14
         * 14     7
         * 15       15
         * */

        public void DrawTo(Color32[] aData, int aWidth, int aHeight, int aXOffset, int aYOffset)
        {

            if (usedColorTable == null || data == null || aData == null)
                return;
            int trColor = -1;
            int bgColorIndex = -1;
            bool useBKColor = graphicControl.DisposalMethod == EDisposalMethod.RestoreBackgroundColor;

            if (Parent.screen.HasGlobalColorTable)
            {
                bgColorIndex = Parent.screen.bgColorIndex;
            }
            if (graphicControl != null && graphicControl.HasTransparentColorIndex)
            {
                
                trColor = graphicControl.transparentColorIndex;
            }
            if (IsInterlaced)
                CalcInterlacedLimits();
            var bgColor = Parent.screen.globalColorTable[bgColorIndex];
            if (Parent.BackgroundTransparent)
                bgColor.a = 0;
            for (int y = 0; y < height; y++)
            {
                int iy = y;
                if (IsInterlaced)
                    iy = GetInterlacedIndex(iy);
                iy = aHeight - iy - yPos - 1;
                //Debug.Log("yPos: " + iy);
                for (int x = 0; x < width; x++)
                {
                    int col = data[x + y * width];
                    int index = xPos + aXOffset + x + (iy + aYOffset) * aWidth;
                    if (col == trColor)
                    {
                        if (useBKColor)
                            //aData[index] = usedColorTable[bgColor];
                            aData[index] = bgColor;
                            
                        continue;
                    }
                    if (index >= 0 && index < aData.Length && col >= 0 && col < usedColorTable.Length)
                        aData[index] = usedColorTable[col];
                }
            }
        }
        public void Dispose(Color32[] aData, int aWidth, int aHeight, int aXOffset, int aYOffset)
        {
            if (graphicControl.DisposalMethod == EDisposalMethod.RestoreBackgroundColor && Parent.screen.HasGlobalColorTable)
            {
                var col = Parent.screen.globalColorTable[Parent.screen.bgColorIndex];
                if (Parent.BackgroundTransparent)
                    col.a = 0;
                for (int y = 0; y < height; y++)
                {
                    int iy = y;
                    iy = aHeight - iy - yPos - 1;
                    for (int x = 0; x < width; x++)
                    {
                        int index = xPos + aXOffset + x + (iy + aYOffset) * aWidth;
                        aData[index] = col;
                    }
                }

            }

        }
    }
    public class GIFTextBlock : IGIFRenderingBlock, IGIFExtension
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.Extension; } }
        public EExtensionType extType { get { return EExtensionType.PlainText; } }
        public GIFGraphicControlExt graphicControl { get; set; }
        public ushort xPos;
        public ushort yPos;
        public ushort width;
        public ushort height;
        public byte charWidth;
        public byte charHeight;
        public byte colorIndex;
        public byte bgColorIndex;
        public string text;
        public void DrawTo(Color32[] aData, int aWidth, int aHeight, int aXOffset, int aYOffset)
        {
            throw new NotImplementedException();
        }
        public void Dispose(Color32[] aData, int aWidth, int aHeight, int aXOffset, int aYOffset)
        {
            throw new NotImplementedException();
        }

    }

    public class GIFCommentBlock : IGIFExtension, IGIFBlock
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.Extension; } }
        public EExtensionType extType { get { return EExtensionType.Comment; } }
        public string text;
    }
    public class GIFApplicationExt : IGIFExtension, IGIFBlock
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.Extension; } }
        public EExtensionType extType { get { return EExtensionType.Application; } }
        public string extName;
        public byte[] data;
    }
    public class GIFGenericExt : IGIFExtension, IGIFBlock
    {
        public GIFImage Parent { get; set; }
        public EBlockType blockType { get { return EBlockType.Extension; } }
        public EExtensionType extType { get; set; }
        public byte[] data;
    }
    #endregion Blocks and extensions



    public class GIFImage
    {
        public bool BackgroundTransparent = false;
        public GIFHeader header;
        public GIFScreenDescriptor screen;
        public List<IGIFBlock> data = new List<IGIFBlock>();
        public List<IGIFRenderingBlock> imageData = new List<IGIFRenderingBlock>();
        public void DrawPartialFrameTo(int aFrame, Color32[] aData, int aWidth, int aHeight, int aXOffset = 0, int aYOffset = 0)
        {
            if (aFrame < 0 || aFrame >= imageData.Count)
                return;
            imageData[aFrame].DrawTo(aData, aWidth, aHeight, aXOffset, aYOffset);
        }
        public void DrawImageTo(int aFrame, Color32[] aData, int aWidth, int aHeight, int aXOffset = 0, int aYOffset = 0)
        {
            if (aFrame < 0 || aFrame >= imageData.Count)
                return;
            for (int i = 0; i <= aFrame; i++)
                imageData[i].DrawTo(aData, aWidth, aHeight, aXOffset, aYOffset);
        }
        public IEnumerator RunAnimation(Action<Texture2D> aOnUpdate, bool aRepeat = true)
        {
            var w = screen.width;
            var h = screen.height;
            var tex = new Texture2D(w, h);
            var colors = new Color32[w * h];
            do
            {
                foreach (var b in imageData)
                {
                    b.DrawTo(colors, w, h);
                    tex.SetPixels32(colors);
                    tex.Apply();
                    aOnUpdate(tex);
                    yield return new WaitForSeconds(b.graphicControl.fdelay);
                    b.Dispose(colors, w, h);
                }
            }
            while (aRepeat);
        }
    }

    public class GIFLoader
    {
        byte[] buf = new byte[255];
        GIFGraphicControlExt lastGrCtrl = null;

        public GIFImage Load(string aFileName)
        {
            using (var stream = File.OpenRead(aFileName))
                return Load(stream);
        }

        public GIFImage Load(Stream aStream)
        {
            using (var reader = new BinaryReader(aStream))
                return Load(reader);
        }

        public GIFImage Load(BinaryReader aReader)
        {
            GIFImage img = new GIFImage();
            if (!ReadFileHeader(aReader, img))
            {
                // Debug.LogWarning("Gif header magic wrong or unknown version: '" + img.header.Magic + "' / '" + img.header.Version + "'");
                return null;
            }
            if (!ReadScreenDescriptor(aReader, img))
            {
                // Debug.LogWarning("Error reading GIF screen descriptor or global color table");
                return null;
            }
            while (true)
            {
                var block = ReadBlock(aReader, img);
                if (block == null)
                    break;
                block.Parent = img;
                img.data.Add(block);
                if (block is IGIFRenderingBlock rBlock)
                    img.imageData.Add(rBlock);
            }
            return img;
        }

        private bool ReadFileHeader(BinaryReader aReader, GIFImage aImage)
        {
            aImage.header.magic = (uint)(aReader.ReadByte() | (aReader.ReadByte() << 8) | (aReader.ReadByte() << 16));
            var ver = aImage.header.version = (uint)(aReader.ReadByte() | (aReader.ReadByte() << 8) | (aReader.ReadByte() << 16));
            return aImage.header.magic == GIFHeader.MAGIC && (ver == GIFHeader.VERSION_87a || ver == GIFHeader.VERSION_89a);
        }
        private bool ReadScreenDescriptor(BinaryReader aReader, GIFImage img)
        {
            img.screen.width = aReader.ReadUInt16();
            img.screen.height = aReader.ReadUInt16();
            img.screen.flags = (EScreenDescriptorFlags)aReader.ReadByte();
            img.screen.bgColorIndex = aReader.ReadByte();
            img.screen.pixelAspectRatio = aReader.ReadByte();
            if (img.screen.HasGlobalColorTable)
                img.screen.globalColorTable = ReadColorTable(aReader, img.screen.SizeOfGlobalColorTable);
            else
                img.screen.globalColorTable = null;
            // if (img.screen.globalColorTable != null)
                // Debug.Log("GBackground: " + img.screen.bgColorIndex+" = " + img.screen.globalColorTable[img.screen.bgColorIndex]);
            return img.screen.HasGlobalColorTable ^ img.screen.globalColorTable == null;
        }

        private IGIFBlock ReadBlock(BinaryReader aReader, GIFImage aImage)
        {
            byte blockType = aReader.ReadByte();
            switch ((EBlockType)blockType)
            {
                case EBlockType.Extension: return ReadExtension(aReader);
                case EBlockType.ImageDescriptor: return ReadImage(aReader, aImage);
                case EBlockType.Trailer: return null;
                default: throw new System.NotSupportedException("Encountered unknown GIF block type: 0x" + blockType.ToString("x2")+" at " +aReader.BaseStream.Position);
            }
        }


        private IGIFBlock ReadExtension(BinaryReader aReader)
        {
            byte extType = aReader.ReadByte();
            switch ((EExtensionType)extType)
            {
                case EExtensionType.GraphicControl: return ReadGraphicControlBlock(aReader);
                case EExtensionType.Comment: return ReadCommentBlock(aReader);
                case EExtensionType.PlainText: return ReadPlainTextBlock(aReader);
                case EExtensionType.Application: return ReadApplicationBlock(aReader);
                default:
                    {
                        // Debug.LogWarning("Encountered unknown extension type: 0x" + extType.ToString("x2") + " at " + aReader.BaseStream.Position);
                        return ReadGenericExtension(aReader, extType);
                    }
            }
        }

        private IGIFBlock ReadGenericExtension(BinaryReader aReader, byte aType)
        {
            var res = new GIFGenericExt();
            res.extType = (EExtensionType)aType;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                byte size = aReader.ReadByte();
                while (size > 0)
                {
                    int read = aReader.Read(buf, 0, size);
                    bw.Write(size);
                    bw.Write(buf, 0, read);
                    size = aReader.ReadByte();
                }
                bw.Flush();
                res.data = ms.ToArray();
            }
            return res;
        }

        private IGIFBlock ReadApplicationBlock(BinaryReader aReader)
        {
            byte appIdentLength = aReader.ReadByte();
            if (appIdentLength != 11)
                throw new Exception("GIF: Application extension identifier block length wrong: " + appIdentLength + " != 11");
            var res = new GIFApplicationExt();
            res.extName = System.Text.Encoding.ASCII.GetString(aReader.ReadBytes(11));
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                byte size = aReader.ReadByte();
                while (size > 0)
                {
                    int read = aReader.Read(buf, 0, size);
                    bw.Write(size);
                    bw.Write(buf, 0, read);
                    size = aReader.ReadByte();
                }
                bw.Flush();
                res.data = ms.ToArray();
            }
            return res;
        }

        private IGIFBlock ReadPlainTextBlock(BinaryReader aReader)
        {
            byte blockSize = aReader.ReadByte();
            if (blockSize != 12)
                throw new Exception("GIF: PlainText extension block size wrong: " + blockSize + " != 12");
            var res = new GIFTextBlock();
            res.xPos = aReader.ReadUInt16();
            res.yPos = aReader.ReadUInt16();
            res.width = aReader.ReadUInt16();
            res.height = aReader.ReadUInt16();
            res.charWidth = aReader.ReadByte();
            res.charHeight = aReader.ReadByte();
            res.colorIndex = aReader.ReadByte();
            res.bgColorIndex = aReader.ReadByte();
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                byte size = aReader.ReadByte();
                while (size > 0)
                {
                    int read = aReader.Read(buf, 0, size);
                    bw.Write(buf, 0, read);
                    size = aReader.ReadByte();
                }
                bw.Flush();
                res.text = System.Text.Encoding.ASCII.GetString(ms.ToArray());
            }
            if (lastGrCtrl != null)
            {
                res.graphicControl = lastGrCtrl;
                lastGrCtrl = null;
            }
            return res;
        }

        private IGIFBlock ReadCommentBlock(BinaryReader aReader)
        {
            var res = new GIFCommentBlock();
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                byte size = aReader.ReadByte();
                while (size > 0)
                {
                    int read = aReader.Read(buf, 0, size);
                    bw.Write(buf, 0, read);
                    size = aReader.ReadByte();
                }
                bw.Flush();
                res.text = System.Text.Encoding.ASCII.GetString(ms.ToArray());
            }
            return res;
        }

        private IGIFBlock ReadGraphicControlBlock(BinaryReader aReader)
        {
            byte blockSize = aReader.ReadByte();
            if (blockSize != 4)
                throw new Exception("GIF: GraphicControl extension block size wrong: " + blockSize + " != 4");
            var res = new GIFGraphicControlExt();
            res.flags = (EGraphicControlFlags)aReader.ReadByte();
            res.delay = aReader.ReadUInt16();
            res.transparentColorIndex = aReader.ReadByte();
            byte nullByte = aReader.ReadByte();
            if (nullByte != 0)
                throw new Exception("GIF: GraphicControl extension block not terminated with '0x00' (found:0x" + nullByte.ToString("x2") + ")");
            lastGrCtrl = res;
            return res;
        }

        private IGIFBlock ReadImage(BinaryReader aReader, GIFImage aImage)
        {
            var res = new GIFImageBlock();
            res.xPos = aReader.ReadUInt16();
            res.yPos = aReader.ReadUInt16();
            res.width = aReader.ReadUInt16();
            res.height = aReader.ReadUInt16();
            res.flags = (EImageDescriptorFlags)aReader.ReadByte();
            if (res.HasLocalColorTable)
            {
                res.usedColorTable =
                res.colorTable = ReadColorTable(aReader, res.SizeOfLocalColorTable);
            }
            else
            {
                res.colorTable = null;
                res.usedColorTable = aImage.screen.globalColorTable;
            }
            if (lastGrCtrl != null)
            {
                res.graphicControl = lastGrCtrl;
                lastGrCtrl = null;
            }
            // use the common method of ignoring the background color if the first frame has a transparent color
            if (res.graphicControl.HasTransparentColorIndex && aImage.imageData.Count == 0)
            {
                aImage.BackgroundTransparent = true;
            }
            ReadLZWImage(aReader, res);
            return res;
        }

        private void ReadLZWImage(BinaryReader aReader, GIFImageBlock res)
        {
            byte initialCodeSize = aReader.ReadByte();
            var bitReader = new BitStreamReader(aReader);
            var output = new List<byte>();
            var dict = new Dict(initialCodeSize);
            long startPos = aReader.BaseStream.Position;
            while (!bitReader.EndOfData)
            {
                int code = (int)bitReader.ReadBits(dict.bits);
                if (bitReader.EndOfData)
                    break;
                if (code == dict.ClearCode)
                    dict.Clear();
                else if (code == dict.EODCode)
                    break;
                else
                {
                    int pre = dict.Expand(code, output);
                    if (pre != -1 && dict.lastCode != -1)
                    {
                        dict.AddPair(dict.lastCode, (byte)pre);
                    }
                    dict.lastCode = code;
                }
            }
            while (!bitReader.EndOfData)
            {
                bitReader.ReadBit();
            }
            res.packedSize = (int)(aReader.BaseStream.Position - startPos);
            res.data = output;
        }

        private Color32[] ReadColorTable(BinaryReader aReader, int aColorCount)
        {
            Color32[] tab = new Color32[aColorCount];
            for (int i = 0; i < aColorCount; i++)
            {
                tab[i] = new Color32(aReader.ReadByte(), aReader.ReadByte(), aReader.ReadByte(), 255);
            }
            return tab;
        }
        internal class BitStreamReader
        {
            BinaryReader m_Reader;
            int m_ChunkSize = -1;
            int m_Pos = 0;
            byte m_Data = 0;
            int m_Bits = 0;
            public bool EndOfData { get { return m_ChunkSize == 0; } }

            public BitStreamReader(BinaryReader aReader)
            {
                m_Reader = aReader;
            }
            public BitStreamReader(Stream aStream) : this(new BinaryReader(aStream)) { }

            public byte ReadBit()
            {
                m_Data >>= 1;
                if (m_Bits <= 0)
                {
                    if (m_Pos >= m_ChunkSize && m_ChunkSize != 0)
                    {
                        m_ChunkSize = m_Reader.ReadByte();
                        m_Pos = 0;
                        if (m_ChunkSize == 0)
                            m_Data = 0;
                    }
                    if (m_Pos < m_ChunkSize)
                    {
                        m_Data = m_Reader.ReadByte();
                        m_Pos++;
                    }
                    m_Bits = 8;
                }
                --m_Bits;
                return (byte)((m_Data) & 1);
            }

            public ulong ReadBits(int aCount)
            {
                ulong val = 0UL;
                if (aCount <= 0 || aCount > 32)
                    throw new System.ArgumentOutOfRangeException("aCount", "aCount must be between 1 and 32 inclusive");
                for (int i = 0; i < aCount; i++)
                    val |= ((ulong)ReadBit() << i);
                return val;
            }
            public void Flush()
            {
                m_Data = 0;
                m_Bits = 0;
                m_ChunkSize = -1;
                m_Pos = 0;
            }
        }
        internal class Dict
        {
            public List<int> list = new List<int>();
            private Stack<byte> stack = new Stack<byte>();
            public int initialBitsPerPixel;
            public int fixedCodes;
            public int bits;
            public int lastCode = -1;
            private int m_ClearCode;
            private int m_EndOfDataCode;
            public int ClearCode { get { return m_ClearCode; } }
            public int EODCode { get { return m_EndOfDataCode; } }
            public Dict(int aBitsPerPixel)
            {
                initialBitsPerPixel = aBitsPerPixel;
                Clear();
            }
            public void Clear()
            {
                fixedCodes = 1 << initialBitsPerPixel;
                bits = initialBitsPerPixel + 1;
                m_ClearCode = fixedCodes;
                m_EndOfDataCode = m_ClearCode + 1;
                list.Clear();
                lastCode = -1;
            }
            internal int GetCode(int aCode)
            {
                if (aCode <= fixedCodes + 1)
                    return -1;
                aCode -= fixedCodes + 2;
                if (aCode >= list.Count)
                    return list.Count - aCode - 2;
                return list[aCode];
            }
            public int GetPrefix(int aCode)
            {
                aCode = GetCode(aCode);
                return aCode >> 8;
            }
            public int GetSuffix(int aCode)
            {
                aCode = GetCode(aCode);
                return aCode & 0xFF;
            }
            public int AddPair(int aPrefix, byte aSuffix)
            {
                list.Add((aPrefix << 8) | aSuffix);
                if (list.Count + fixedCodes + 2 >= 1 << bits)
                {
                    if (bits < 12)
                        bits++;
                }
                return list.Count - 1;

            }
            public int Expand(int aCode, List<byte> aData)
            {
                int c = aCode;
                stack.Clear();
                while (c > fixedCodes)
                {
                    int newC = GetCode(c);
                    if (newC == -1)
                        break;
                    else if (newC == -2)
                    {
                        if (stack.Count > 0 || lastCode == -1)
                            throw new Exception("Bad code found in LZW stream.");
                        int tmp = lastCode;
                        byte newSuf = (byte)Expand(tmp, aData);
                        aData.Add(newSuf);
                        AddPair(tmp, newSuf);
                        lastCode = aCode;
                        return -1;
                    }
                    else if (newC < -2)
                        throw new Exception("Bad code found in LZW stream");
                    int pre = newC >> 8;
                    byte suf = (byte)(newC & 0xFF);
                    stack.Push(suf);
                    c = pre;
                }
                byte ret = (byte)c;
                aData.Add((byte)c);
                if (stack.Count > 0)
                {
                    while (stack.Count > 0)
                        aData.Add(stack.Pop());
                }
                return ret;
            }
        }
    }
}