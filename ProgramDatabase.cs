﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace StandaloneOrganizr
{
	public class ProgramDatabase
	{
		private readonly string Filename;
		private readonly List<ProgramLink> programs = new List<ProgramLink>();

		private bool isUpdating = false;

		public ProgramDatabase(string file)
		{
			Filename = file;

			if (File.Exists(file))
			{
				Load(file);
			}
		}

		public void Add(ProgramLink prog)
		{
			programs.Add(prog);

			Save();
		}

		public void BeginUpdate()
		{
			isUpdating = true;
		}

		public void EndUpdate()
		{
			isUpdating = false;
			Save();
		}

		public void Load(string path)
		{
			var data = File.ReadAllText(path, Encoding.UTF8);

			var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

			for (var i = 0; (i + 1) < lines.Length; i += 2)
			{
				programs.Add(new ProgramLink(lines[i] + Environment.NewLine + lines[i + 1], i));
			}
		}

		private string SaveToString()
		{
			return string.Join(Environment.NewLine, programs.Select(p => p.Save()));
		}

		private List<SearchResult> FindKeyword(string search)
		{
			return programs
				.Select(p => new SearchResult(p) { Score = p.Find(search) })
				.ToList();
		}

		private IEnumerable<SearchResult> FindRegex(string regex)
		{
			return FindRegex(new Regex(regex));
		}

		private IEnumerable<SearchResult> FindRegex(Regex regex)
		{
			return programs
				.Select(p => new SearchResult(p) { Score = p.Find(regex) })
				.ToList();
		}

		private IEnumerable<SearchResult> FindCommand(string cmd)
		{
			cmd = cmd.ToLower();

			if (cmd == "e" || cmd == "empty")
			{
				return List().Where(p => p.Keywords.Count == 0).Select(p => new SearchResult(p));
			}

			if (cmd == "a" || cmd == "all")
			{
				return List().Select(p => new SearchResult(p));
			}

			if (cmd == "n" || cmd == "new")
			{
				return List().Where(p => p.IsNew).Select(p => new SearchResult(p));
			}

			return Enumerable.Empty<SearchResult>();
		}

		public bool ContainsFolder(string path)
		{
			return programs.Any(p =>
			{
				var fileName = Path.GetFileName(path);
				return fileName != null && p.Directory.ToLower() == fileName.ToLower();
			});
		}

		public void Save()
		{
			if (isUpdating) return;

			try
			{
				File.WriteAllText(Filename, SaveToString(), Encoding.UTF8);
			}
			catch (IOException)
			{
				Thread.Sleep(500);

				File.WriteAllText(Filename, SaveToString(), Encoding.UTF8);
			}
		}

		public IEnumerable<ProgramLink> List()
		{
			return programs.AsEnumerable();
		}

		public bool Remove(ProgramLink rem)
		{
			var result = programs.Remove(rem);

			if (result) Save();

			return result;
		}

		public IEnumerable<SearchResult> Find(string searchterm)
		{
			if (string.IsNullOrWhiteSpace(searchterm))
				return Enumerable.Empty<SearchResult>();

			if (searchterm.StartsWith(":"))
				return FindCommand(searchterm.Substring(1));

			if (searchterm.StartsWith("/") && searchterm.EndsWith("/"))
				return FindRegex(searchterm.Substring(1, searchterm.Length-2));

			return FindKeyword(searchterm);
		}

		public void Clear()
		{
			programs.Clear();
			Save();
		}
	}
}