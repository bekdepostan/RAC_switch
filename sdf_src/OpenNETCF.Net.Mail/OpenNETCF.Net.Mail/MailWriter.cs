﻿using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenNETCF.Net.Mime;
using System.Collections.Specialized;
using System.Net.Sockets;

namespace OpenNETCF.Net.Mail
{
    internal class MailWriter
    {
        // Fields
        private BufferBuilder bufferBuilder;
        private Stream contentStream;
        private static byte[] CRLF = new byte[] { 13, 10 };
        private static int DefaultLineLength = 0x4e;
        private bool isInContent;
        private int lineLength;
        private EventHandler onCloseHandler;
        private Stream stream;

        // Methods
        internal MailWriter(Stream stream)
            : this(stream, DefaultLineLength)
        {
        }

        internal MailWriter(Stream stream, int lineLength)
        {
            this.bufferBuilder = new BufferBuilder();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (lineLength < 0)
            {
                throw new ArgumentOutOfRangeException("lineLength");
            }
            this.stream = stream;
            this.lineLength = lineLength;
            this.onCloseHandler = new EventHandler(this.OnClose);
        }

        internal void Close()
        {
            this.stream.Write(CRLF, 0, 2);
            this.stream.Close();
        }

        private void Flush()
        {
            if (this.bufferBuilder.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine(ASCIIEncoding.ASCII.GetString(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length));
                this.stream.Write(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length);
                this.bufferBuilder.Reset();
            }
        }

        internal Stream GetContentStream()
        {
            return this.GetContentStream(ContentTransferEncoding.SevenBit);
        }

        private Stream GetContentStream(ContentTransferEncoding contentTransferEncoding)
        {
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            this.isInContent = true;
            this.bufferBuilder.Append(CRLF);
            this.Flush();
            Stream stream = this.stream;
            if (contentTransferEncoding == ContentTransferEncoding.SevenBit)
            {
                stream = new SevenBitStream(stream);
            }
            else if (contentTransferEncoding == ContentTransferEncoding.QuotedPrintable)
            {
                stream = new QuotedPrintableStream(stream, this.lineLength);
            }
            else if (contentTransferEncoding == ContentTransferEncoding.Base64)
            {
                stream = new Base64Stream(stream, this.lineLength);
            }
            ClosableStream stream2 = new ClosableStream(stream, this.onCloseHandler);
            this.contentStream = stream2;
            return stream2;
        }

        private void OnClose(object sender, EventArgs args)
        {
            this.contentStream.Flush();
            this.contentStream = null;
        }

       
        private void WriteAndFold(string value)
        {
            if (value.Length < DefaultLineLength)
            {
                this.bufferBuilder.Append(value);
            }
            else
            {
                int offset = 0;
                int length = value.Length;
                while ((length - offset) > DefaultLineLength)
                {
                    int num3 = value.LastIndexOf(' ', (offset + DefaultLineLength) - 1, DefaultLineLength - 1);
                    if (num3 > -1)
                    {
                        this.bufferBuilder.Append(value, offset, num3 - offset);
                        this.bufferBuilder.Append(CRLF);
                        offset = num3;
                    }
                    else
                    {
                        this.bufferBuilder.Append(value, offset, DefaultLineLength);
                        offset += DefaultLineLength;
                    }
                }
                if (offset < length)
                {
                    this.bufferBuilder.Append(value, offset, length - offset);
                }
            }
        }

        internal void WriteHeader(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            this.bufferBuilder.Append(name);
            this.bufferBuilder.Append(": ");
            this.WriteAndFold(value);
            this.bufferBuilder.Append(CRLF);
        }

        internal void WriteHeaders(NameValueCollection headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            foreach (string str in headers)
            {
                foreach (string str2 in headers.GetValues(str))
                {
                    this.WriteHeader(str, str2);
                }
            }
        }
    }
}