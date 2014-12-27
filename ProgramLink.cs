using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StandaloneOrganizr
{
	public class ProgramLink
	{
		public string name = "";
		public string directory = "";
		public List<string> keywords = new List<string>();

		public ProgramLink()
		{
		}

		public ProgramLink(string src)
		{
			Load(src);
		}

		public void Load(string data)
		{
			string[] lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			if (lines.Length != 2)
				throw new IOException();

			string[] head = lines[0].Split(':');
			if (head.Length != 2)
				throw new IOException();

			name = head[0].Trim();
			directory = Regex.Unescape(head[1].Trim().Trim('"'));
			keywords = lines[1].Trim().Split(' ').ToList();
		}

		public string Save()
		{
			return name + ": \"" + Regex.Escape(directory) + "\"" + Environment.NewLine + "\t" + string.Join(" ", keywords);
		}

		public int Find(string search)
		{
			if (name.ToLower().Contains(search.ToLower()))
				return 2;

			if (keywords.Any(p => p.ToLower().Contains(search.ToLower())))
				return 1;

			return 0;
		}

		public override string ToString()
		{
			return name;
		}
	}
}
