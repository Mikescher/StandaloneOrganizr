
namespace StandaloneOrganizr
{
	public class SearchResult
	{
		public readonly ProgramLink Program;
		public int Score /* = 0 */;

		public SearchResult(ProgramLink p, int score)
		{
			Program = p;
			Score = score;
		}

		public override string ToString()
		{
			return Program.ToString();
		}
	}
}
