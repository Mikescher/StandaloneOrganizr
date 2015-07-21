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

		public int GetSearchScore(string search)
		{
			int score = 0;

			if (Name.ToLower() == search.ToLower())
				score += 10;

			if (Name.ToLower().Contains(search.ToLower()))
				score += 2;

			score += 4 * Keywords.Count(p => p.ToLower() == search.ToLower());

			score += 1 * Keywords.Count(p => p.ToLower().Contains(search.ToLower()));

			return score;
		}

		public int GetSearchScore(Regex regex)
		{
			int score = 0;

			if (regex.IsMatch(Name))
				score += 2;

			score += Keywords.Count(regex.IsMatch);

			return score;
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

		public void Start(FileSystemScanner scanner)
		{
			string exec = scanner.FindExecutable(this);

			if (exec != null)
			{
				Process.Start(exec);
			}
			else
			{
				Process.Start("explorer.exe", GetAbsolutePath(scanner.GetRootPath()));
			}

		}

		public string GetAbsolutePath(string rootPath)
		{
			return Path.Combine(rootPath, Directory);
		}
	}
}
