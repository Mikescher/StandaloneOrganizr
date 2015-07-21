using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StandaloneOrganizr
{
	class FileSystemScanner
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
					db.Add(new ProgramLink
					{
						Directory = miss,
						Name = miss,
						Keywords = new List<string>(),
					});
				}

				db.EndUpdate();
			}
		}
	}
}
