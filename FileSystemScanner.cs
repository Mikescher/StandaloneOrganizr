using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents.Serialization;

namespace StandaloneOrganizr
{
	public class FileSystemScanner
	{
		private readonly string folderPath;

		public FileSystemScanner(string path)
		{
			folderPath = path;
		}

		public void Scan(ProgramDatabase db, out List<ProgramLink> removed, out List<string> missing)
		{
			var directories = Directory.GetDirectories(folderPath);

			missing = directories
				.Select(Path.GetFileName)
				.Where(p => !db.ContainsFolder(p))
				.ToList();

			removed = db.List()
				.Where(p => directories.All(q => (Path.GetFileName(q) ?? string.Empty).ToLower() != p.Directory.ToLower()))
				.ToList();

			if (missing.Any())
			{
				db.BeginUpdate();

				foreach (var miss in missing)
				{
					db.Add(new ProgramLink(this)
					{
						Directory = miss,
						Name = miss,
						Keywords = new List<string>(),
					});
				}

				db.EndUpdate();
			}
		}

		public string FindExecutable(ProgramLink prog)
		{
			string progPath = prog.GetAbsolutePath(folderPath);
			string result;

			//#########################

			result = FindRedirectInFolder(progPath, prog);
			if (result != null) return result;

			//#########################

			result = FindExecutableInFolder(progPath, prog);
			if (result != null) return result;

			//#########################

			result = FindJarInFolder(progPath, prog);
			if (result != null) return result;

			//#########################

			var binPath = Path.Combine(progPath, "bin");
			if (Directory.Exists(binPath))
			{
				result = FindExecutableInFolder(binPath, prog);
				if (result != null) return result;
			}

			//#########################

			var folders = Directory.GetDirectories(progPath);
			if (folders.Count() == 1)
			{
				result = FindExecutableInFolder(folders[0], prog);
				if (result != null) return result;
			}

			//#########################

			return null;
		}

		private string FindExecutableInFolder(string path, ProgramLink prog)
		{
			var executables = Directory
				.EnumerateFiles(path)
				.Where(f => (Path.GetExtension(f) ?? "err").ToLower() == ".exe")
				.ToList();

			if (executables.Count == 1)
			{
				return executables.First();
			}

			if (executables.Any(f => (Path.GetFileNameWithoutExtension(f) ?? "").ToLower() == prog.Name.ToLower()))
			{
				return executables.First(f => (Path.GetFileNameWithoutExtension(f) ?? "").ToLower() == prog.Name.ToLower());
			}

			return null;
		}

		private string FindJarInFolder(string path, ProgramLink prog)
		{
			var executables = Directory
				.EnumerateFiles(path)
				.Where(f => (Path.GetExtension(f) ?? "err").ToLower() == ".jar")
				.ToList();

			if (executables.Count == 1)
			{
				return executables.First();
			}

			if (executables.Any(f => (Path.GetFileNameWithoutExtension(f) ?? "").ToLower() == prog.Name.ToLower()))
			{
				return executables.First(f => (Path.GetFileNameWithoutExtension(f) ?? "").ToLower() == prog.Name.ToLower());
			}

			return null;
		}

		private string FindRedirectInFolder(string path, ProgramLink prog)
		{
			var redirects = Directory
				.EnumerateFiles(path)
				.Where(f => (Path.GetExtension(f) ?? "err").ToLower() == ".sao-redirect")
				.Select(f => Path.Combine(Path.GetDirectoryName(f) ?? "", File.ReadAllLines(f).FirstOrDefault() ?? ""))
				.Where(File.Exists)
				.ToList();

			if (redirects.Any())
			{
				return redirects.First();
			}

			return null;
		}

		public string GetRootPath()
		{
			return folderPath;
		}
	}
}
