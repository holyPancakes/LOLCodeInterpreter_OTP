using System;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//created to show a syntax error for the user
	public class SyntaxException : Exception 
	{
		public SyntaxException () : base(){}

		public SyntaxException (string message) : base (message){}
	}
}

