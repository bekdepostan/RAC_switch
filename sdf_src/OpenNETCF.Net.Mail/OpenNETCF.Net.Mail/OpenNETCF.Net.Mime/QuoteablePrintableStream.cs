using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenNETCF.Net.Mail;

namespace OpenNETCF.Net.Mime
{
    internal class QuotedPrintableStream : DelegatedStream
    {
        // Fields
        private static int DefaultLineLength = 0x4c;
        private bool encodeCRLF;
        private static byte[] hexDecodeMap = new byte[] { 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
     };
        private static byte[] hexEncodeMap = new byte[] { 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 70 };
        private int lineLength;
        private ReadStateInfo readState;
        private WriteStateInfo writeState;

        // Methods
        internal QuotedPrintableStream()
        {
            this.lineLength = DefaultLineLength;
        }

        internal QuotedPrintableStream(int lineLength)
        {
            this.lineLength = lineLength;
        }

        internal QuotedPrintableStream(Stream stream, bool encodeCRLF)
            : this(stream, DefaultLineLength)
        {
            this.encodeCRLF = encodeCRLF;
        }

        internal QuotedPrintableStream(Stream stream, int lineLength)
            : base(stream)
        {
            if (lineLength < 0)
            {
                throw new ArgumentOutOfRangeException("lineLength");
            }
            this.lineLength = lineLength;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
            //if (buffer == null)
            //{
            //    throw new ArgumentNullException("buffer");
            //}
            //if ((offset < 0) || (offset > buffer.Length))
            //{
            //    throw new ArgumentOutOfRangeException("offset");
            //}
            //if ((offset + count) > buffer.Length)
            //{
            //    throw new ArgumentOutOfRangeException("count");
            //}
            //WriteAsyncResult result = new WriteAsyncResult(this, buffer, offset, count, callback, state);
            //result.Write();
            //return result;
        }

        public override void Close()
        {
            this.FlushInternal();
            base.Close();
        }

        internal unsafe int DecodeBytes(byte[] buffer, int offset, int count)
        {
            fixed (byte* numRef = buffer)
            {
                try
                {

                    byte* numPtr = numRef + offset;
                    byte* numPtr2 = numPtr;
                    byte* numPtr3 = numPtr;
                    byte* numPtr4 = numPtr + count;
                    if (this.ReadState.IsEscaped)
                    {
                        if (this.ReadState.Byte == -1)
                        {
                            if (count == 1)
                            {
                                this.ReadState.Byte = numPtr2[0];
                                return 0;
                            }
                            if ((numPtr2[0] != 13) || (numPtr2[1] != 10))
                            {
                                byte num = hexDecodeMap[numPtr2[0]];
                                byte num2 = hexDecodeMap[numPtr2[1]];
                                if (num == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num }));
                                }
                                if (num2 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num2 }));
                                }
                                numPtr3++;
                                numPtr3[0] = (byte)((num << 4) + num2);
                            }
                            numPtr2 += 2;
                        }
                        else
                        {
                            if ((this.ReadState.Byte != 13) || (numPtr2[0] != 10))
                            {
                                byte num3 = hexDecodeMap[this.ReadState.Byte];
                                byte num4 = hexDecodeMap[numPtr2[0]];
                                if (num3 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num3 }));
                                }
                                if (num4 == 0xff)
                                {
                                    throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num4 }));
                                }
                                numPtr3++;
                                numPtr3[0] = (byte)((num3 << 4) + num4);
                            }
                            numPtr2++;
                        }
                        this.ReadState.IsEscaped = false;
                        this.ReadState.Byte = -1;
                    }
                    while (numPtr2 < numPtr4)
                    {
                        if (numPtr2[0] != 0x3d)
                        {
                            numPtr3++;
                            numPtr2++;
                            numPtr3[0] = numPtr2[0];
                            continue;
                        }
                        long num8 = (long)((numPtr4 - numPtr2) / 1);
                        if ((num8 <= 2L) && (num8 >= 1L))
                        {
                            switch (((int)(num8 - 1L)))
                            {
                                case 0:
                                    goto Label_0215;

                                case 1:
                                    this.ReadState.Byte = numPtr2[1];
                                    goto Label_0215;
                            }
                        }
                        goto Label_0226;
                    Label_0215:
                        this.ReadState.IsEscaped = true;
                        break;
                    Label_0226:
                        if ((numPtr2[1] != 13) || (numPtr2[2] != 10))
                        {
                            byte num5 = hexDecodeMap[numPtr2[1]];
                            byte num6 = hexDecodeMap[numPtr2[2]];
                            if (num5 == 0xff)
                            {
                                throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num5 }));
                            }
                            if (num6 == 0xff)
                            {
                                throw new FormatException(SR.GetString("InvalidHexDigit", new object[] { num6 }));
                            }
                            numPtr3++;
                            numPtr3[0] = (byte)((num5 << 4) + num6);
                        }
                        numPtr2 += 3;
                    }
                    count = (int)((long)((numPtr3 - numPtr) / 1));
                }
                finally
                {
                }
            }
            return count;
        }

        internal int EncodeBytes(byte[] buffer, int offset, int count)
        {
            int index = offset;
            while (index < (count + offset))
            {
                if (((this.lineLength != -1) && ((this.WriteState.CurrentLineLength + 5) >= this.lineLength)) && (((buffer[index] == 0x20) || (buffer[index] == 9)) || ((buffer[index] == 13) || (buffer[index] == 10))))
                {
                    int num2;
                    int num3;
                    int num4;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 3)
                    {
                        return (index - offset);
                    }
                    this.WriteState.CurrentLineLength = 0;
                    WriteStateInfo writeState = this.WriteState;
                    writeState.Length = (num2 = writeState.Length) + 1;
                    this.WriteState.Buffer[num2] = 0x3d;
                    WriteStateInfo info2 = this.WriteState;
                    info2.Length = (num3 = info2.Length) + 1;
                    this.WriteState.Buffer[num3] = 13;
                    WriteStateInfo info3 = this.WriteState;
                    info3.Length = (num4 = info3.Length) + 1;
                    this.WriteState.Buffer[num4] = 10;
                }
                if ((this.WriteState.CurrentLineLength == 0) && (buffer[index] == 0x2e))
                {
                    int num5;
                    WriteStateInfo info4 = this.WriteState;
                    info4.Length = (num5 = info4.Length) + 1;
                    this.WriteState.Buffer[num5] = 0x2e;
                }
                if (((buffer[index] == 13) && ((index + 1) < (count + offset))) && (buffer[index + 1] == 10))
                {
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < (this.encodeCRLF ? 6 : 2))
                    {
                        return (index - offset);
                    }
                    index++;
                    if (this.encodeCRLF)
                    {
                        int num6;
                        int num7;
                        int num8;
                        int num9;
                        int num10;
                        int num11;
                        WriteStateInfo info5 = this.WriteState;
                        info5.Length = (num6 = info5.Length) + 1;
                        this.WriteState.Buffer[num6] = 0x3d;
                        WriteStateInfo info6 = this.WriteState;
                        info6.Length = (num7 = info6.Length) + 1;
                        this.WriteState.Buffer[num7] = 0x30;
                        WriteStateInfo info7 = this.WriteState;
                        info7.Length = (num8 = info7.Length) + 1;
                        this.WriteState.Buffer[num8] = 0x44;
                        WriteStateInfo info8 = this.WriteState;
                        info8.Length = (num9 = info8.Length) + 1;
                        this.WriteState.Buffer[num9] = 0x3d;
                        WriteStateInfo info9 = this.WriteState;
                        info9.Length = (num10 = info9.Length) + 1;
                        this.WriteState.Buffer[num10] = 0x30;
                        WriteStateInfo info10 = this.WriteState;
                        info10.Length = (num11 = info10.Length) + 1;
                        this.WriteState.Buffer[num11] = 0x41;
                        WriteStateInfo info11 = this.WriteState;
                        info11.CurrentLineLength += 6;
                    }
                    else
                    {
                        int num12;
                        int num13;
                        WriteStateInfo info12 = this.WriteState;
                        info12.Length = (num12 = info12.Length) + 1;
                        this.WriteState.Buffer[num12] = 13;
                        WriteStateInfo info13 = this.WriteState;
                        info13.Length = (num13 = info13.Length) + 1;
                        this.WriteState.Buffer[num13] = 10;
                        this.WriteState.CurrentLineLength = 0;
                    }
                }
                else if (((buffer[index] < 0x20) && (buffer[index] != 9)) || ((buffer[index] == 0x3d) || (buffer[index] > 0x7e)))
                {
                    int num14;
                    int num15;
                    int num16;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 3)
                    {
                        return (index - offset);
                    }
                    WriteStateInfo info14 = this.WriteState;
                    info14.CurrentLineLength += 3;
                    WriteStateInfo info15 = this.WriteState;
                    info15.Length = (num14 = info15.Length) + 1;
                    this.WriteState.Buffer[num14] = 0x3d;
                    WriteStateInfo info16 = this.WriteState;
                    info16.Length = (num15 = info16.Length) + 1;
                    this.WriteState.Buffer[num15] = hexEncodeMap[buffer[index] >> 4];
                    WriteStateInfo info17 = this.WriteState;
                    info17.Length = (num16 = info17.Length) + 1;
                    this.WriteState.Buffer[num16] = hexEncodeMap[buffer[index] & 15];
                }
                else
                {
                    int num17;
                    if ((this.WriteState.Buffer.Length - this.WriteState.Length) < 1)
                    {
                        return (index - offset);
                    }
                    WriteStateInfo info18 = this.WriteState;
                    info18.CurrentLineLength++;
                    WriteStateInfo info19 = this.WriteState;
                    info19.Length = (num17 = info19.Length) + 1;
                    this.WriteState.Buffer[num17] = buffer[index];
                }
                index++;
            }
            return (index - offset);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
            //WriteAsyncResult.End(asyncResult);
        }

        public override void Flush()
        {
            this.FlushInternal();
            base.Flush();
        }

        private void FlushInternal()
        {
            if ((this.writeState != null) && (this.writeState.Length > 0))
            {
                base.Write(this.WriteState.Buffer, 0, this.WriteState.Length);
                this.WriteState.Length = 0;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int num = 0;
            while (true)
            {
                num += this.EncodeBytes(buffer, offset + num, count - num);
                if (num >= count)
                {
                    break;
                }
                this.FlushInternal();
            }
        }

        // Properties
        private ReadStateInfo ReadState
        {
            get
            {
                if (this.readState == null)
                {
                    this.readState = new ReadStateInfo();
                }
                return this.readState;
            }
        }

        internal WriteStateInfo WriteState
        {
            get
            {
                if (this.writeState == null)
                {
                    this.writeState = new WriteStateInfo(0x400);
                }
                return this.writeState;
            }
        }

        // Nested Types
        private class ReadStateInfo
        {
            // Fields
            private short b1 = -1;
            private bool isEscaped;

            // Properties
            internal short Byte
            {
                get
                {
                    return this.b1;
                }
                set
                {
                    this.b1 = value;
                }
            }

            internal bool IsEscaped
            {
                get
                {
                    return this.isEscaped;
                }
                set
                {
                    this.isEscaped = value;
                }
            }
        }

        internal class WriteStateInfo
        {
            // Fields
            private byte[] buffer;
            private int currentLineLength;
            private int length;

            // Methods
            internal WriteStateInfo(int bufferSize)
            {
                this.buffer = new byte[bufferSize];
            }

            // Properties
            internal byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }

            internal int CurrentLineLength
            {
                get
                {
                    return this.currentLineLength;
                }
                set
                {
                    this.currentLineLength = value;
                }
            }

            internal int Length
            {
                get
                {
                    return this.length;
                }
                set
                {
                    this.length = value;
                }
            }
        }
    }
}