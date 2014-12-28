
namespace StandaloneOrganizr
{
	public class SearchResult
	{
		public readonly ProgramLink program;
		public int score = 0;

		public SearchResult(ProgramLink p)
		{
			this.program = p;
		}

		public override string ToString()
		{
			return program.ToString();
		}
	}
}
