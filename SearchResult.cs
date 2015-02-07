
namespace StandaloneOrganizr
{
	public class SearchResult
	{
		public readonly ProgramLink program;
		public int Score /* = 0 */;

		public SearchResult(ProgramLink p)
		{
			program = p;
		}

		public override string ToString()
		{
			return program.ToString();
		}

		public void Start()
		{
			program.Start();
		}
	}
}
