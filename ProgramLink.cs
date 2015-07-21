using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StandaloneOrganizr
{
	public class ProgramLink
	{
		public string Name = "";
		public string Directory = "";
		public List<string> Keywords = new List<string>();
		public bool IsNew;

		public ProgramLink()
		{
			IsNew = true;
		}

		public ProgramLink(string src, int line)
		{
			Load(src, line);
			IsNew = false;
		}

		private void Load(string data, int line)
		{
			var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			if (lines.Length != 2)
				throw new Exception("[ERR_2001] Invalid db file syntax in line " + line);

			var head = lines[0].Split(':');
			if (head.Length != 2)
				throw new Exception("[ERR_2002] Invalid db file syntax in line " + line);

			Name = head[0].Trim();
			Directory = UnescapeStr(head[1].Trim());

			if (!(Directory.StartsWith("\"") && Directory.EndsWith("\"")))
				throw new Exception("[ERR_2003] Invalid db file syntax in line " + (line + 1));

			Directory = Directory.Substring(1, Directory.Length - 2);

			Keywords = lines[1].Trim().Split(' ').Where(p => p.Trim() != "").Select(p => p.ToLower()).Distinct().ToList();
		}

		public string Save()
		{
			return Name + ": \"" + EscapeStr(Directory) + "\"" + Environment.NewLine + "\t" + string.Join(" ", Keywords);
		}

		public int Find(string search)
		{
			if (Name.ToLower().Contains(search.ToLower()))
				return 2;

			return Keywords.Any(p => p.ToLower().Contains(search.ToLower())) ? 1 : 0;
		}

		public int Find(Regex regex)
		{
			if (regex.IsMatch(Name))
				return 2;

			return Keywords.Any(regex.IsMatch) ? 1 : 0;
		}

		private static string EscapeStr(string value)
		{
			const char backSlash = '\\';
			const char slash = '/';
			const char dblQuote = '"';

			var output = new StringBuilder(value.Length);
			foreach (var c in value)
			{
				switch (c)
				{
					case slash:
						output.AppendFormat("{0}{1}", backSlash, slash);
						break;

					case backSlash:
						output.AppendFormat("{0}{0}", backSlash);
						break;

					case dblQuote:
						output.AppendFormat("{0}{1}", backSlash, dblQuote);
						break;

					default:
						output.Append(c);
						break;
				}
			}

			return output.ToString();
		}

		private static string UnescapeStr(string value)
		{
			const char backSlash = '\\';

			var output = new StringBuilder(value.Length);
			var esc = false;
			foreach (var c in value)
			{
				if (esc)
				{
					output.Append(c);
					esc = false;
				}
				else
				{
					switch (c)
					{
						case backSlash:
							esc = true;
							output.Append(c);
							break;

						default:
							output.Append(c);
							break;
					}
				}
			}

			return output.ToString();
		}

		public override string ToString()
		{
			return Name;
		}

		public void Start()
		{
			Process.Start("explorer.exe", Path.GetFullPath(Directory));
		}
	}
}
