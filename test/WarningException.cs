using System;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//for debugging purposes only. shows warning when something wrong went in the code
	public class WarningException : Exception 
	{
		public WarningException () : base(){}

		public WarningException (string message) : base (message){}
	}
}

