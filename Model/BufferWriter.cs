using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


class BufferWriter: BinaryWriter
{
    public BufferWriter(Stream output) : base(output) { }
    public BufferWriter(Stream output, Encoding encoding) { }

    public void Dispose()
    {
        this.BaseStream.Dispose();
        base.Dispose(true);
    }
}

class BufferReader: BinaryReader
{
    public BufferReader(Stream input) : base(input) { }
    public BufferReader(Stream input, Encoding encoding) : base(input, encoding) { }

    public void Dispose()
    {
        this.BaseStream.Dispose();
        base.Dispose(true);
    }
}