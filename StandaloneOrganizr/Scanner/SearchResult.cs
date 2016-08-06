
using MSHC.MVVM;

namespace StandaloneOrganizr.Scanner
{
	public class SearchResult : ObservableObject
	{
		private readonly ProgramLink _program;
		public int Score /* = 0 */;

		public ProgramLink Program => _program;

		public SearchResult(ProgramLink p, int score)
		{
			_program = p;
			Score = score;
		}

		public override string ToString()
		{
			return _program.ToString();
		}
	}
}
