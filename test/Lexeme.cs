using System;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//class for lexemes in a code (token and the description)
	public class Lexeme
	{

		private String name; //lexeme name/keyword
		private String description; //description which describes the lexeme

		public Lexeme(){
			this.name = "!"; //default value when a lexeme has no value
		}

		public Lexeme(String n, String desc)
		{ //constructor
			this.name = n; //initializes the name
			this.description = desc; //initializes the description
		}

		//getters
		public String getName() //gets the lexeme name
		{
			return this.name;
		}

		public String getDescription() //gets the description
		{
			return this.description;
		}

		//converts the object to string
		public String toString()
		{
			return "[" + this.name + ":" + this.description + "]";
		}
	}
}

