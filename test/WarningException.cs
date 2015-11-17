using System;

namespace test
{
	public class WarningException : Exception 
	{
		public WarningException () : base(){}

		public WarningException (string message) : base (message){}
	}
}

