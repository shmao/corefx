// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    internal class XmlBuffer
    {
        private List<Section> _sections;
        private byte[] _buffer;
        private int _offset;
        private BufferedStream _stream;
        private BufferState _bufferState;
        private XmlDictionaryWriter _writer;
        private XmlDictionaryReaderQuotas _quotas;

        private enum BufferState
        {
            Created,
            Writing,
            Reading,
        }

        private struct Section
        {
            private int _offset;
            private int _size;
            private XmlDictionaryReaderQuotas _quotas;

            public Section(int offset, int size, XmlDictionaryReaderQuotas quotas)
            {
                _offset = offset;
                _size = size;
                _quotas = quotas;
            }

            public int Offset
            {
                get { return _offset; }
            }

            public int Size
            {
                get { return _size; }
            }

            public XmlDictionaryReaderQuotas Quotas
            {
                get { return _quotas; }
            }
        }

        public XmlBuffer(int maxBufferSize)
        {
            if (maxBufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(maxBufferSize), maxBufferSize, SR.ValueMustBeNonNegative);
            
            int initialBufferSize = Math.Min(512, maxBufferSize); 
            _stream = new BufferedStream(new MemoryStream(), initialBufferSize);
            _sections = new List<Section>(1);
        }

        public int BufferSize
        {
            get
            {
                return _buffer.Length;
            }
        }

        public int SectionCount
        {
            get { return _sections.Count; }
        }

        public XmlDictionaryWriter OpenSection(XmlDictionaryReaderQuotas quotas)
        {
            if (_bufferState != BufferState.Created)
                throw CreateInvalidStateException();
            _bufferState = BufferState.Writing;
            _quotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(_quotas);

            _writer = XmlDictionaryWriter.CreateBinaryWriter(_stream, null, null, false);

            return _writer;
        }

        public void CloseSection()
        {
            if (_bufferState != BufferState.Writing)
                throw CreateInvalidStateException();
            _writer.Close();
            _bufferState = BufferState.Created;

            int size = (int)_stream.Length - _offset;
            _sections.Add(new Section(_offset, size, _quotas));
            _offset += size;
        }

        public void Close()
        {
            if (_bufferState != BufferState.Created)
                throw CreateInvalidStateException();
            _bufferState = BufferState.Reading;
            _buffer = new byte[_stream.Length];
            _stream.Position = 0;
            _stream.Read(_buffer, 0, _buffer.Length);

            _writer = null;
            _stream = null;
        }

        private Exception CreateInvalidStateException()
        {
            return new InvalidOperationException(SR.XmlBufferInInvalidState);
        }


        public XmlDictionaryReader GetReader(int sectionIndex)
        {
            if (_bufferState != BufferState.Reading)
                throw CreateInvalidStateException();
            Section section = _sections[sectionIndex];
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(_buffer, section.Offset, section.Size, null, section.Quotas, null, null);

            reader.MoveToContent();
            return reader;
        }

        public void WriteTo(int sectionIndex, XmlWriter writer)
        {
            if (_bufferState != BufferState.Reading)
                throw CreateInvalidStateException();
            XmlDictionaryReader reader = GetReader(sectionIndex);
            try
            {
                writer.WriteNode(reader, false);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
