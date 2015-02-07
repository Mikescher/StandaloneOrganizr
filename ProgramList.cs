using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace StandaloneOrganizr
{
	public class ProgramList
	{
		public readonly List<ProgramLink> Programs = new List<ProgramLink>();

		public void Load(string data)
		{
			var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

			for (var i = 0; (i + 1) < lines.Length; i += 2)
			{
				Programs.Add(new ProgramLink(lines[i] + Environment.NewLine + lines[i + 1], i));
			}
		}

		private string Save()
		{
			return string.Join(Environment.NewLine, Programs.Select(p => p.Save()));
		}

		public List<SearchResult> Find(string search)
		{
			return Programs
				.Select(p => new SearchResult(p) { Score = p.Find(search) })
				.ToList();
		}

		public IEnumerable<SearchResult> Find(Regex regex)
		{
			return Programs
				.Select(p => new SearchResult(p) { Score = p.Find(regex) })
				.ToList();
		}

		public bool ContainsFolder(string path)
		{
			return Programs.Any(p =>
			{
				var fileName = Path.GetFileName(path);
				return fileName != null && p.Directory.ToLower() == fileName.ToLower();
			});
		}

		public void Update(string fn)
		{
			try
			{
				File.WriteAllText(fn, Save(), Encoding.UTF8);
			}
			catch (IOException)
			{
				Thread.Sleep(500);

				File.WriteAllText(fn, Save(), Encoding.UTF8);
			}
		}
	}
}
