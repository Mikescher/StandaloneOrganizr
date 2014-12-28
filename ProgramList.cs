using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace StandaloneOrganizr
{
	public class ProgramList
	{
		public List<ProgramLink> programs = new List<ProgramLink>();

		public ProgramList()
		{

		}

		public ProgramList(string src)
		{
			Load(src);
		}

		public void Load(string data)
		{
			string[] lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

			for (int i = 0; (i + 1) < lines.Length; i += 2)
			{
				programs.Add(new ProgramLink(lines[i] + Environment.NewLine + lines[i + 1]));
			}
		}

		public string Save()
		{
			return string.Join(Environment.NewLine, programs.Select(p => p.Save()));
		}

		public List<SearchResult> find(string search)
		{
			return programs
				.Select(p => new SearchResult(p) { score = p.Find(search) })
				.ToList();
		}

		public bool ContainsFolder(string path)
		{
			return programs.Any(p => p.directory.ToLower() == Path.GetFileName(path).ToLower());
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
