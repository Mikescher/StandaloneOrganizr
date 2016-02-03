using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using StandaloneOrganizr.IconUtils;

namespace StandaloneOrganizr
{
	public class ProgramLink
	{
		public Guid id;
		public string Name = "";
		public string Directory = "";
		public int Priority;
		public List<string> Keywords = new List<string>();
		public bool IsNew;

		private readonly FileSystemScanner Scanner;

		public ImageSource Icon
		{
			get
			{
				if (CacheUpToDate) return CachedImage;

				UpdateCache();

				return CachedImage;
			}
		}

		public string Executable
		{
			get
			{
				if (CacheUpToDate) return CachedExecutable;

				UpdateCache();

				return CachedExecutable;
			}
		}

		private bool CacheUpToDate = false;
		private ImageSource CachedImage = null;
		private string CachedExecutable = null;

		public ProgramLink(FileSystemScanner scanner)
		{
			IsNew = true;
			id = Guid.NewGuid();

			Scanner = scanner;
		}

		public ProgramLink(FileSystemScanner scanner, string src, int line, Dictionary<Guid, int> priorities)
		{
			Load(src, line, priorities);
			IsNew = false;

			Scanner = scanner;
		}

		private void Load(string data, int line, Dictionary<Guid, int> priorities)
		{
			var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			if (lines.Length != 2)
				throw new Exception("[ERR_2001] Invalid db file syntax in line " + line);

			var head = lines[0].Split(':');
			if (head.Length != 2)
				throw new Exception("[ERR_2002] Invalid db file syntax in line " + line);

			id = Guid.Parse(string.Join(string.Empty, head[1].ToCharArray().Reverse().Take(38).Reverse()));
			head[1] = head[1].Substring(0, head[1].Length - 38).Trim();

			Priority = 0;
			if (priorities.ContainsKey(id)) Priority = priorities[id];

			Name = head[0].Trim();
			Directory = UnescapeStr(head[1].Trim());

			if (!(Directory.StartsWith("\"") && Directory.EndsWith("\"")))
				throw new Exception("[ERR_2003] Invalid db file syntax in line " + (line + 1));

			Directory = Directory.Substring(1, Directory.Length - 2);

			Keywords = lines[1].Trim().Split(' ').Where(p => p.Trim() != "").Select(p => p.ToLower()).Distinct().ToList();
		}

		public string Save_Database()
		{
			return string.Format("{0}: \"{1}\" ({3})\r\n\t{2}", Name, EscapeStr(Directory), string.Join(" ", Keywords), id.ToString("D").ToUpper());
		}

		public string Save_Priority()
		{
			return string.Format("{0} > {1}", id.ToString("B").ToUpper(), Priority);
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

		public void Start(ProgramDatabase d)
		{
			Priority++;

			if (Executable != null)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = Executable,
					WorkingDirectory = Path.GetDirectoryName(Executable) ?? "",
				});
			}
			else
			{
				Process.Start("explorer.exe", GetAbsolutePath(Scanner.GetRootPath()));
			}

			d.Save();
		}

		public string GetAbsolutePath(string rootPath)
		{
			return Path.Combine(rootPath, Directory);
		}

		private void UpdateCache()
		{
			var exec = Scanner.FindExecutable(this);
			if (exec != null)
			{
				CachedExecutable = exec;

				try
				{
					var extr = new IconExtractor(exec);

					CachedImage = extr.Count == 0 ? null : IconUtil.ToImageSource(extr.GetIcon(extr.Count - 1));
				}
				catch (Exception)
				{
					CachedImage = null;
				}
			}
			else
			{
				CachedExecutable = null;
				CachedImage = null;
			}

			CacheUpToDate = true;
		}
	}
}
