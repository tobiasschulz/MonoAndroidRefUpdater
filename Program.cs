//
// iOS platform assembly reference updater.
//
// Copyright 2013 Xamarin Inc
//
// Authors:
//    Tobias Schulz  <tobiasschulz.dev@outlook.com>
//    Sebastien Pouliot  <sebastien@xamarin.com>
//    Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//

using System;
using System.IO;
using Mono.Cecil;

class AssemblyReferenceUpdater
{
	static byte[] new_pk_token = null;
	//{ 0x84, 0xe0, 0x4f, 0xf9, 0xcf, 0xb7, 0x90, 0x65 };

	static int Usage (string message)
	{
		Console.WriteLine ("Assembly Reference Update for Mono.Android");
		Console.WriteLine ("Usage: MonoAndroidRefUpdater assembly.dll");
		Console.WriteLine ();
		Console.WriteLine ("Error: {0}", message);
		return 1;
	}

	static int Error (string message)
	{
		Console.WriteLine ("Error: {0}", message);
		return 1;
	}

	public static int Main (string[] args)
	{
		if (args.Length == 0)
			return Usage ("No assembly was specified.");
		string filename = args [0];
		if (!File.Exists (filename))
			return Error (String.Format ("Could not find `{0}`.", filename));
		
		string backup = args [0] + ".bak";
		if (File.Exists (backup))
			return Error (String.Format ("Backup file `{0}` is already present.", backup));
		
		try {
			File.Copy (args [0], backup);

			var resolver = new DefaultAssemblyResolver ();
			resolver.AddSearchDirectory (Path.GetDirectoryName (args [0]));
			var parameters = new ReaderParameters {
				AssemblyResolver = resolver,
			};
			
			bool action = false;
			var ad = AssemblyDefinition.ReadAssembly (args [0], parameters);
			foreach (var reference in ad.MainModule.AssemblyReferences) {
				switch (reference.FullName) {
				case "Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065":
					action = true;
					Console.WriteLine ("Updating reference for {0}", reference.Name);
					reference.PublicKeyToken = new_pk_token; 
					break;
				}
			}
			if (action) {
				if (ad.Name.HasPublicKey)
					Console.WriteLine ("warning: any existing assembly signature will be invalid.");
				ad.Write (args [0]);
			} else {
				Console.WriteLine ("No reference needed to be modified. Original file is unchanged.");
			}
			return 0;
		} catch (BadImageFormatException) {
			return Error ("Not a .NET assembly!");
		} catch (Exception e) {
			return Error (e.ToString ());
		}
	}
}