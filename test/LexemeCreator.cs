using System;
using GLib;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace test
{
	public class LexemeCreator
	{
		private Boolean quotedDouble;
		private String str;
		private String token;

		public LexemeCreator ()
		{
			quotedDouble = false;
		}

		public List<Lexeme> process(String line)
		{ //processes a string

			List<Lexeme> lex = new List<Lexeme>();

			foreach(char c in line)
			{ //converts it to array of characters
				if(c == '\t')
				{ 
					continue; //ignore if it is a tab
				}
				else if(c == ' ' && !quotedDouble)
				{ //checks if the character is space and not quoted
					if(token.EndsWith(" ") || token.Length == 0) continue; //ignore if it is a space
					checker(lex, c); //else checks the token if it is a keyword or a value
				}
				else if(c == '\"')
				{ //checks if the char is a double quotes
					quotedDouble = !quotedDouble; //switches the flags of double quotes
					if(!quotedDouble)
					{ //if the double quote becomes false
						Lexeme temp = new Lexeme(str, "YARN constant"); //creates a lexeme
						lex.Add(temp); //adds the lexeme to an array of lexemes
						str = ""; //resets the string
					}
				}
				else if(quotedDouble)
				{ 
					str += c; //append the char to string
				}
				else
				{
					token+=c;//appends the char to token otherwise
				}
			}

			if(quotedDouble)
			{//checks if the string is properly closed
				throw new Exception("String not properly closed.");
			}
			checker(lex, '\n'); //checks the token of it is not yet empty

			if(token.Length!=0)
			{ //checks if the token is not a keyword and found dangling in the code
				throw new Exception(string.Concat(token,": keyword not found."));
			}

			return lex; //returns the array of lexemes
		}

		private void checker(List<Lexeme> lex, char c)
		{ //checks the token if it is a keyword or a value
			String varIdent = "[A-Za-z][A-Za-z0-9_]*"; //regex for variable
			String numbr = "\\d+"; //regex for numbr
			String numbar = "(\\d+|\\d*\\.\\d+)"; //regex for numbar
			String troof = "(WIN|FAIL)"; //regex for troof
			Lexeme temp = new Lexeme();
			Regex rxVarIdent = new Regex (varIdent, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Regex rxvNumbr = new Regex (numbr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Regex rxNumbar = new Regex (numbar, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Regex rxTroof = new Regex (troof, RegexOptions.Compiled | RegexOptions.IgnoreCase);


			if(rxvNumbr.IsMatch(token))
			{ //checks if it is a NUMBER constant
				temp = new Lexeme(token, "NUMBR constant");
				lex.Add(temp);
				token = "";
			}
			else if(rxNumbar.IsMatch(token))
			{ //checks if it is a NUMBAR constant
				temp = new Lexeme(token, "NUMBAR constant");
				lex.Add(temp);
				token = "";
			}
			else if(rxTroof.IsMatch(token))
			{ //checks if it is a TROOF constant
				temp = new Lexeme(token, "TROOF constant");
				lex.Add(temp);
				token = "";
			}
			else if(token.Equals("HAI"))
			{ //checks if it is a HAI keyword
				temp = new Lexeme("HAI", "Starts a program");
				lex.Add(temp);
				token = "";
			}
			else if(token.Equals("KTHXBYE"))
			{ //checks if it is a KTHXBYE keyword
				temp = new Lexeme("KTHXBYE", "Ends a program");
				lex.Add(temp);
				token = "";
			}
			else if(token.Equals("VISIBLE"))
			{ //checks if the keyword is VISIBLE
				temp = new Lexeme("VISIBLE", "Prints a value"); //forms a lexeme
				lex.Add(temp); //adds it to an array of lexemes
				token = ""; //resets the keyword
			}
			else if(token.Equals("ITZ"))
			{ //checks if it is a ITZ keyword
				temp = new Lexeme("ITZ", "Assigns value after declaration.");
				lex.Add(temp);
				token = "";
			}
			else if(token.StartsWith("I"))
			{ //checks if it is possibly a I HAS A keyword
				if(!token.Equals("I HAS A")){
					token += c;
					return;
				}
				temp = new Lexeme("I HAS A", "Declares a variable.");
				lex.Add(temp);
				token = "";
			}
			else if(token.Equals("R"))
			{ //checks if it is a R keyword
				temp = new Lexeme("R", "Assignment operation");
				lex.Add(temp);
				token = "";
			}
			else if(rxVarIdent.IsMatch(token))
			{ //checks if it is a variable
				temp = new Lexeme(token, "Variable Identifier");
				lex.Add(temp);
				token = "";
			}
			else if(c!='\n')
			{ //checks if it is already at the end of the line
				if(token.Length!=0) token += c;
			}
		}

		public void reset()
		{ //resets the attributes of scanner
			quotedDouble = false;
			str = "";
			token = "";
		}
	}
}

