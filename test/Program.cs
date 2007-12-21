﻿// Copyright (c) 2007 Frank Laub
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
using System.Collections.Generic;
using System.Text;
using OpenSSL;

namespace test
{
	interface ICommand
	{
		void Execute(string[] args);
	}

	#region NullCommand
	class NullCommand : ICommand
	{
		private string name;
		public NullCommand(string name)
		{
			this.name = name;
		}

		#region ICommand Members
		public void Execute(string[] args)
		{
			Console.WriteLine("{0}: {1}", this.name, string.Join(" ", args));
			Console.WriteLine("Not implemented yet!");
		}
		#endregion
	}
	#endregion

	class Program
	{
		static void Main(string[] args)
		{
			Program program = new Program();
			program.Run(args);
		}
		SortedDictionary<string, ICommand> tests = new SortedDictionary<string, ICommand>();

		void AddNullCommand(SortedDictionary<string, ICommand> map, string name)
		{
			map.Add(name, new NullCommand(name));
		}

		Program()
		{
			tests.Add("dh", new TestDH());
			tests.Add("dsa", new TestDSA());
			tests.Add("sha1", new TestSHA1());
			tests.Add("sha", new TestSHA());
			tests.Add("sha256", new TestSHA256());
			tests.Add("sha512", new TestSHA512());
			tests.Add("rsa", new TestRSA());

			AddNullCommand(tests, "bf");
			AddNullCommand(tests, "bn");
			AddNullCommand(tests, "cast");
			AddNullCommand(tests, "des");
			AddNullCommand(tests, "dummy");
			AddNullCommand(tests, "ecdh");
			AddNullCommand(tests, "ecdsa");
			AddNullCommand(tests, "ec");
			AddNullCommand(tests, "engine");
			AddNullCommand(tests, "evp");
			AddNullCommand(tests, "exp");
			AddNullCommand(tests, "hmac");
			AddNullCommand(tests, "idea");
			AddNullCommand(tests, "ige");
			AddNullCommand(tests, "md2");
			AddNullCommand(tests, "md4");
			AddNullCommand(tests, "md5");
			AddNullCommand(tests, "mdc2");
			AddNullCommand(tests, "meth");
			AddNullCommand(tests, "r160");
			AddNullCommand(tests, "rand");
			AddNullCommand(tests, "rc2");
			AddNullCommand(tests, "rc4");
			AddNullCommand(tests, "rc5");
			AddNullCommand(tests, "rmd");
		}

		void PrintCommands(IEnumerable<string> cmds)
		{
			int col = 0;
			foreach (string cmd in cmds)
			{
				Console.Write(cmd);
				if (col++ == 4)
				{
					Console.WriteLine();
					col = 0;
					continue;
				}

				int remain = 15 - cmd.Length;
				string padding = new string(' ', remain);
				Console.Write(padding);
			}
			Console.WriteLine();
		}

		void Usage()
		{
			PrintCommands(tests.Keys);
		}

		void Run(string[] args)
		{
			if (args.Length == 0)
			{
				Usage();
				return;
			}

			ICommand cmd;
			if(!this.tests.TryGetValue(args[0], out cmd))
			{
				Usage();
				return;
			}

			MemoryTracker.Start();
			cmd.Execute(args);
			MemoryTracker.Finish();
		}
	}

	public class MemoryTracker
	{
		private static int leaked = 0;

		public static void Start()
		{
			Native.CRYPTO_malloc_debug_init();
			Native.CRYPTO_dbg_set_options(Native.V_CRYPTO_MDEBUG_ALL);
			Native.CRYPTO_mem_ctrl(Native.CRYPTO_MEM_CHECK_ON);
		}

		public static void Finish()
		{
			Native.CRYPTO_cleanup_all_ex_data();
			Native.ERR_remove_state(0);

			GC.Collect();
			Native.CRYPTO_mem_leaks_cb(new Native.CRYPTO_MEM_LEAK_CB(OnMemoryLeak));
			if (leaked > 0)
				Console.WriteLine("Leaked total bytes: {0}", leaked);
		}

		private static void OnMemoryLeak(uint order, string file, int line, int num_bytes, IntPtr addr)
		{
			Console.WriteLine("file: {1} line: {2} bytes: {3}", order, file, line, num_bytes);
			leaked += num_bytes;
		}
	}
}
