using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OpenSSL
{
	public class BIO : Base, IDisposable
	{
		#region Initialization
		internal BIO(IntPtr ptr, bool owner) : base(ptr, owner) { }

		public BIO(byte[] buf) 
			: base(Native.ExpectNonNull(Native.BIO_new_mem_buf(buf, buf.Length)), true)
		{
		}

		public BIO(string str)
			: this(Encoding.ASCII.GetBytes(str))
		{
		}

		public static BIO MemoryBuffer()
		{
			IntPtr ptr = Native.ExpectNonNull(Native.BIO_new(Native.BIO_s_mem()));
			return new BIO(ptr, true);
		}

		public static BIO File(string filename, string mode)
		{
			byte[] bufFilename = Encoding.ASCII.GetBytes(filename);
			byte[] bufMode = Encoding.ASCII.GetBytes(mode);
			IntPtr ptr = Native.ExpectNonNull(Native.BIO_new_file(bufFilename, bufMode));
			return new BIO(ptr, true);
		}

		private const int FD_STDIN = 0;
		private const int FD_STDOUT = 1;
		private const int FD_STDERR = 2;

		public static BIO MessageDigest(MessageDigest md)
		{
			IntPtr ptr = Native.ExpectNonNull(Native.BIO_new(Native.BIO_f_md()));
			Native.BIO_set_md(ptr, md.Handle);
			return new BIO(ptr, true);
		}

		//public static BIO MessageDigestContext(MessageDigestContext ctx)
		//{
		//    IntPtr ptr = Native.ExpectNonNull(Native.BIO_new(Native.BIO_f_md()));
		//    //IntPtr ptr = Native.ExpectNonNull(Native.BIO_new(Native.BIO_f_null()));
		//    Native.BIO_set_md_ctx(ptr, ctx.Handle);
		//    return new BIO(ptr);
		//}
		#endregion

		#region Properties
		public uint NumberRead
		{
			get { return Native.BIO_number_read(this.Handle); }
		}

		public uint NumberWritten
		{
			get { return Native.BIO_number_written(this.Handle); }
		}
		#endregion

		#region Methods
		public void Push(BIO bio)
		{
			Native.ExpectNonNull(Native.BIO_push(this.ptr, bio.Handle));
		}

		public void Write(byte[] buf)
		{
			if (Native.BIO_write(this.ptr, buf, buf.Length) != buf.Length)
				throw new OpenSslException();
		}

		public void Write(byte[] buf, int len)
		{
			if (Native.BIO_write(this.ptr, buf, len) != len)
				throw new OpenSslException();
		}

		public void Write(byte value)
		{
			byte[] buf = new byte[1];
			buf[0] = value;
			Write(buf);
		}

		public void Write(ushort value)
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter br = new BinaryWriter(ms);
			br.Write(value);
			byte[] buf = ms.ToArray();
			Write(buf);
		}

		public void Write(uint value)
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter br = new BinaryWriter(ms);
			br.Write(value);
			byte[] buf = ms.ToArray();
			Write(buf);
		}

		public void Write(string str)
		{
			byte[] buf = Encoding.ASCII.GetBytes(str);
			if (Native.BIO_puts(this.ptr, buf) != buf.Length)
				throw new OpenSslException();
		}

		public ArraySegment<byte> ReadBytes(int count)
		{
			byte[] buf = new byte[count];
			int ret = Native.BIO_read(this.ptr, buf, buf.Length);
			if (ret < 0)
				throw new OpenSslException();

			return new ArraySegment<byte>(buf, 0, ret);
		}

		public string ReadString()
		{
			StringBuilder sb = new StringBuilder();
			const int BLOCK_SIZE = 64;
			byte[] buf = new byte[BLOCK_SIZE];
			int ret = 0;
			while(true)
			{
				ret = Native.BIO_gets(this.ptr, buf, buf.Length);
				if (ret == 0)
					break;
				if (ret < 0)
					throw new OpenSslException();

				sb.Append(Encoding.ASCII.GetString(buf, 0, ret));
			}
			return sb.ToString();
		}

		public MessageDigestContext GetMessageDigestContext()
		{
			return new MessageDigestContext(this);
		}

		#endregion

		#region IDisposable Members

		public override void OnDispose()
		{
			Native.BIO_free(this.ptr);
		}

		#endregion
	}
}
