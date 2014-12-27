using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StandaloneOrganizr
{
	public class ProgramLink
	{
		public string name = "";
		public string directory = "";
		public List<string> keywords = new List<string>();
		public bool newly = false;

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
				throw new Exception("Invalid db file syntax");

			string[] head = lines[0].Split(':');
			if (head.Length != 2)
				throw new Exception("Invalid db file syntax");

			name = head[0].Trim();
			directory = Regex.Unescape(head[1].Trim());

			if (!(directory.StartsWith("\"") && directory.EndsWith("\"")))
				throw new Exception("Invalid db file syntax");

			directory = directory.Substring(1, directory.Length - 2);

			keywords = lines[1].Trim().Split(' ').Where(p => p.Trim() != "").ToList();
		}

		public string Save()
		{
			return name + ": \"" + EscapeStr(directory) + "\"" + Environment.NewLine + "\t" + string.Join(" ", keywords);
		}

		public int Find(string search)
		{
			if (name.ToLower().Contains(search.ToLower()))
				return 2;

			if (keywords.Any(p => p.ToLower().Contains(search.ToLower())))
				return 1;

			return 0;
		}

		public static string EscapeStr(string value)
		{
			const char BACK_SLASH = '\\';
			const char SLASH = '/';
			const char DBL_QUOTE = '"';

			var output = new StringBuilder(value.Length);
			foreach (char c in value)
			{
				switch (c)
				{
					case SLASH:
						output.AppendFormat("{0}{1}", BACK_SLASH, SLASH);
						break;

					case BACK_SLASH:
						output.AppendFormat("{0}{0}", BACK_SLASH);
						break;

					case DBL_QUOTE:
						output.AppendFormat("{0}{1}", BACK_SLASH, DBL_QUOTE);
						break;

					default:
						output.Append(c);
						break;
				}
			}

			return output.ToString();
		}

		public static string UnescapeStr(string value)
		{
			const char BACK_SLASH = '\\';
			const char SLASH = '/';
			const char DBL_QUOTE = '"';

			var output = new StringBuilder(value.Length);
			bool esc = false;
			foreach (char c in value)
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
						case BACK_SLASH:
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
			return name;
		}
	}
}
