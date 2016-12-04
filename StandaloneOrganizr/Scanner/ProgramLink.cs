using MSHC.MVVM;
using StandaloneOrganizr.IconUtils;
using StandaloneOrganizr.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace StandaloneOrganizr.Scanner
{
	public class ProgramLink : ObservableObject
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
				if (cacheUpToDate) return cachedImage;

				UpdateCache(true);

				return cachedImage;
			}
		}

		public string Executable
		{
			get
			{
				if (cacheUpToDate) return cachedExecutable;

				UpdateCache(false);

				return cachedExecutable;
			}
		}

		public string ExecutableCached
		{
			get
			{
				if (cacheUpToDate) return cachedExecutable;

				UpdateCache(true);

				return cachedExecutable;
			}
		}

		private bool cacheUpToDate = false;
		private ImageSource cachedImage = null;
		private string cachedExecutable = null;

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
			if (!App.DebugMode) Priority++;

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

		private void UpdateCache(bool async)
		{
			if (async)
			{
				BackgroundQueue.Inst.QueueTask(UpdateCacheInternal);
			}
			else
			{
				UpdateCacheInternal();
			}
		}

		private void UpdateCacheInternal()
		{
			string newCachedExecutable;
			Icon newCachedImage;

			//Thread.Sleep(2000);

			var exec = Scanner.FindExecutable(this);
			if (exec != null)
			{
				newCachedExecutable = exec;

				try
				{
					var extr = new IconExtractor(exec);

					if (extr.Count == 0)
					{
						newCachedImage = null;
					}
					else
					{
						newCachedImage = extr.GetIcon(extr.Count - 1);
					}
				}
				catch (Exception)
				{
					newCachedImage = null;
				}
			}
			else
			{
				newCachedExecutable = null;
				newCachedImage = null;
			}


			if (Application.Current.Dispatcher.CheckAccess())
			{
				cachedExecutable = newCachedExecutable;
				cachedImage = IconUtil.ToImageSource(newCachedImage);
				cacheUpToDate = true;

				OnPropertyChanged("Icon");
			}
			else
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					cachedExecutable = newCachedExecutable;
					cachedImage = IconUtil.ToImageSource(newCachedImage);
					cacheUpToDate = true;

					OnPropertyChanged("Icon");
				});
			}

		}
	}
}
