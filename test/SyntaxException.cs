using System;

namespace test
{
	public class SyntaxException : Exception 
	{
		public SyntaxException () : base(){}

		public SyntaxException (string message) : base (message){}
	}
}

