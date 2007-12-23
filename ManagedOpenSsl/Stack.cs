// Copyright (c) 2006-2007 Frank Laub
// All rights reserved.

// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. The name of the author may not be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenSSL
{
	/// <summary>
	/// Simple interface used for the internal implementation of the generic OpenSSL.Stack
	/// </summary>
	public interface IStackable
	{
		/// <summary>
		/// Underlying native pointer
		/// </summary>
		IntPtr Handle { get; set; }
	}

	/// <summary>
	/// Encapsultes the sk_* functions
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Stack<T> : Base, IDisposable, IList<T> where T : IStackable, new()
	{
		#region Initialization
		internal Stack(IntPtr ptr, bool owner) : base(ptr, owner) { }

		/// <summary>
		/// Calls sk_new_null()
		/// </summary>
		public Stack() : base(Native.ExpectNonNull(Native.sk_new_null()), true) {}

		/// <summary>
		/// Calls sk_shift()
		/// </summary>
		/// <returns></returns>
		public T Shift()
		{
			T item = new T();
			item.Handle = Native.sk_shift(this.ptr);
			return item;
		}
		#endregion

		#region Enumerator
		class Enumerator : IEnumerator<T>
		{
			private Stack<T> parent;
			private int index = -1;
			public Enumerator(Stack<T> parent)
			{
				this.parent = parent;
			}

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if (this.index < 0 || this.index >= this.parent.Count)
						throw new InvalidOperationException();

					IntPtr ptr = Native.ExpectNonNull(Native.sk_value(this.parent.Handle, index));
					T item = new T();
					item.Handle = ptr;
					return item;
				}
			}

			#endregion

			#region IDisposable Members
			public void Dispose()
			{
			}
			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { return this.Current; }
			}

			public bool MoveNext()
			{
				this.index++;
				if (this.index < this.parent.Count)
					return true;
				return false;
			}

			public void Reset()
			{
				this.index = -1;
			}

			#endregion
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Calls sk_free()
		/// </summary>
		public override void OnDispose()
		{
			Native.sk_free(this.ptr);
		}
		#endregion

		#region IList<T> Members

		/// <summary>
		/// Returns sk_find()
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item)
		{
			return Native.sk_find(this.ptr, item.Handle);
		}

		/// <summary>
		/// Calls sk_insert()
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, T item)
		{
			Native.ExpectSuccess(Native.sk_insert(this.ptr, item.Handle, index));
		}

		/// <summary>
		/// Calls sk_delete()
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			IntPtr ptr = Native.ExpectNonNull(Native.sk_delete(this.ptr, index));
		}

		/// <summary>
		/// Indexer that returns sk_value() or calls sk_insert()
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{
				IntPtr ptr = Native.ExpectNonNull(Native.sk_value(this.ptr, index));
				T item = new T();
				item.Handle = ptr;
				return item;
			}
			set
			{
				int ret = Native.sk_insert(this.ptr, value.Handle, index);
				if (ret < 0)
					throw new OpenSslException();
			}
		}

		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// Calls sk_push()
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{
			if (Native.sk_push(this.ptr, item.Handle) <= 0)
				throw new OpenSslException();
		}

		/// <summary>
		/// Calls sk_zero()
		/// </summary>
		public void Clear()
		{
			Native.sk_zero(this.ptr);
		}

		/// <summary>
		/// Returns true if the specified item exists in this stack.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item)
		{
			foreach (T element in this)
			{
				if (element.Equals(item))
					return true;
			}
			return false;
			//int ret = Native.sk_find(this.ptr, item.Handle);
			//if (ret >= 0 && ret < this.Count)
			//    return true;
			//return false;
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Returns sk_num()
		/// </summary>
		public int Count
		{
			get
			{
				int ret = Native.sk_num(this.ptr);
				if (ret < 0)
					throw new OpenSslException();
				return ret;
			}
		}

		/// <summary>
		/// Returns false.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Calls sk_delete_ptr()
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{
			IntPtr ptr = Native.sk_delete_ptr(this.ptr, item.Handle);
			if (ptr != IntPtr.Zero)
				return true;
			return false;
		}

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// Returns an enumerator for this stack
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion
	}
}
