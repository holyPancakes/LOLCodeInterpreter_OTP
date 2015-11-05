using System;

namespace test
{
	public class Lexeme
	{

		private String name; //lexeme name/keyword
		private String description; //description which describes the lexeme

		public Lexeme(){}

		public Lexeme(String n, String desc)
		{ //constructor
			this.name = n;
			this.description = desc;
		}

		//getters
		public String getName()
		{
			return this.name;
		}

		public String getDescription()
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

