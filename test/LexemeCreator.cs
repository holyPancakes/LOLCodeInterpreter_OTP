using System;
using GLib;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//given the whole code, creates a list of lexemes
	public class LexemeCreator
	{
		private Boolean quotedDouble; //checks if a double quote is seen in the code
		private Boolean isComment; //checks if the current character is inside a multi-line comment 
		private Boolean oneLineComment; //checks if a character is inside a one line comment
		private String str; //string formed inside a double quote
		private String token; //token formed inside the quote

		public LexemeCreator (){ //initial values of attributes
			quotedDouble = false;
			isComment = false;
			oneLineComment = false;
			str = "";
			token = "";
		}

		public List<Lexeme> process(String[] lines){ //lines is the code divided in a line
			List<Lexeme> lex = new List<Lexeme>();

			foreach (String line in lines) { //for each line in the source code
				foreach (char c in line) { //for each character in a line
					if (c == '\t' && !quotedDouble) //ignote tabs provided they are not inside double quotes
						continue;
					else if ((c == ' ' || c == Constants.SOFTBREAKCHAR || c == '!') && !quotedDouble) { //checks if an important character is found
						if ((token.EndsWith (" ") || token.Length == 0) && c == ' ') //checks if the token already has spaces in it
							continue; //do not add it so that spaces will not be redundant in the token
						else if (token.Equals (Constants.ONELINE)) { //checks if it is a one line comment
							if (!isComment) { //checks if it is inside a multiline comment
								Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment"); //create a new lexeme
								oneLineComment = true; //switch the boolean on
								lex.Add (temp); //add the new lexeme to the list of lexemes
								token = ""; //resets the token
							}
						} else if (token.Equals (Constants.MULTILINE)) { //checks if it is a multiline comment
							Lexeme temp = new Lexeme (Constants.MULTILINE, "Multiline comment"); //creates a new lexeme
							lex.Add (temp); //adds it to the list of lexeme
							isComment = true; //switches the boolean on
							token = ""; //resets the token
						} else if (token.Equals (Constants.ENDCOMMENT)) { //checks if it is the end of a comment
							if (isComment) { //checks if multiline comment is still going on
								Lexeme temp = new Lexeme (Constants.ENDCOMMENT, "Ends the comment"); //adds the new lexeme
								lex.Add (temp); //adds the new lexeme to a list of lexemes
								isComment = false; //switches the boolean off
								token = ""; //resets the token
							} else //else no start of multiline comment is found. alerts the user about the syntax error
								throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDCOMMENT));
						} else if (isComment || oneLineComment) //resets the token if it is a comment
							token = "";
						else //else checks for more keyword that might match the token
							checker (lex, c);
						
						if (!isComment && !oneLineComment) { //checks if the important character is not inside the comment
							//if not adds the character to a list of lexemes
							if (c == '!')
								lex.Add (new Lexeme (Constants.NONEWLINE, "Makes " + Constants.PRINT + " print with no new line."));
							if (c == Constants.SOFTBREAKCHAR)
								lex.Add (new Lexeme (Constants.SOFTBREAK, "Soft-command break"));
						}
					} else if (c == '\"') { //if double quotes is found
						if (isComment || oneLineComment) //continues if it is inside a comment
							continue;
						else if (str.EndsWith (":")) { //continues if it is included in an escape charcater
							str += c;
							continue;
						}
						quotedDouble = !quotedDouble; //switches the boolean
						if (!quotedDouble) { //if switched off, adds the string constant to the list of lexemes
							Lexeme temp = new Lexeme (str, Constants.STRING + " constant");
							lex.Add (temp);
							lex.Add (new Lexeme ("\"", "YARN Delimiter")); //also adds the string delimiter
							str = ""; //resets the string
						} else{
							lex.Add (new Lexeme ("\"", "YARN Delimiter")); //else adds the string delimiter only
						}
					} else if (quotedDouble){ //else if the double quote is present, adds the character to the string
						str += c;
					} else{ //else adds the character to the current token
						token += c;
					}
				} //end of checking character by character in a line

				//after the line if there is leftover token
				if (quotedDouble) //if double quote is found, alerts the user that the double quote is not properly closed
					throw new SyntaxException (WarningMessage.lackDoubleQuote ());
				if (token.Equals (Constants.ONELINE)) { //checks if it is a online comment and adds it to a list of lexemes
					Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment");
					lex.Add (temp);
					isComment = true;
					token = "";
				} else if (token.Equals (Constants.MULTILINE)) { //checks if it is a start of multiline comment and adds it to a list of lexemes
					Lexeme temp = new Lexeme (Constants.MULTILINE, "Multiline comment");
					lex.Add (temp);
					isComment = true;
					token = "";
				} else if (token.Equals (Constants.ENDCOMMENT)) { //checks if it is a end of multiline comment and adds it to lexemes
					if (isComment) {
						Lexeme temp = new Lexeme (Constants.ENDCOMMENT, "Ends the comment");
						lex.Add (temp);
						isComment = false;
						token = "";
					} else //shows a syntax error if the end of comment is found but start of comment is not found
						throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDCOMMENT));
				} else if (isComment || oneLineComment) //if it is a comment resets the token
					token = "";
				else //checks the token for additional keywords
					checker (lex, '\n');

				if (token.Length != 0) { //if the token did not matched any keywords shows that syntax error happened
					throw new SyntaxException (WarningMessage.unexpectedLexeme (token));
				}

				oneLineComment = false; //switches the online comment off
				lex.Add(new Lexeme(Constants.EOL, "Hard-command break")); //adds a lexeme to mark the end of line
			} //end of checking line by line in the source code

			return lex; //after processing all of the source code returns the list of lexeme
		}

		private void checker(List<Lexeme> lex, char c){ //checks if the token matches any keyword
			Lexeme temp = new Lexeme();
			char[] delimiter = { ' ' }; //for splitting token by spaces

			//checks if the token matches ANY of these one word keywords
			if (Constants.INTVAL.IsMatch (token))
				temp = new Lexeme (token, Constants.INT + " constant");
			else if (Constants.FLOATVAL.IsMatch (token))
				temp = new Lexeme (token, Constants.FLOAT + " constant");
			else if (Constants.BOOLVAL.IsMatch (token))
				temp = new Lexeme (token, Constants.BOOL + " constant");
			else if (token.Equals (Constants.STARTPROG))
				temp = new Lexeme (Constants.STARTPROG, "Starts a program");
			else if (token.Equals (Constants.ENDPROG))
				temp = new Lexeme (Constants.ENDPROG, "Ends a program");
			else if (token.Equals (Constants.PRINT))
				temp = new Lexeme (Constants.PRINT, "Prints a value");
			else if (token.Equals (Constants.SCAN))
				temp = new Lexeme (Constants.SCAN, "Scans a value");
			else if (token.Equals (Constants.CASE))
				temp = new Lexeme (Constants.CASE, "Case on Switch-case");
			else if (token.Equals (Constants.SWITCH))
				temp = new Lexeme (Constants.SWITCH, "Switch on Switch-case");
			else if (token.Equals (Constants.CASE))
				temp = new Lexeme (Constants.CASE, "Case on Switch-case");
			else if (token.Equals (Constants.DEFAULT))
				temp = new Lexeme (Constants.DEFAULT, "Default on Switch-case");
			else if (token.Equals (Constants.BREAK))
				temp = new Lexeme (Constants.BREAK, "Goes out of switch-case");
			else if (token.Equals (Constants.AN))
				temp = new Lexeme (Constants.AN, "Separates arguments");
			else if (token.Equals (Constants.NOT))
				temp = new Lexeme (Constants.NOT, "Operator NOT that negates the condition");
			else if (token.Equals (Constants.MKAY))
				temp = new Lexeme (Constants.MKAY, "Closes the arity or concat");
			else if (token.Equals (Constants.CONCAT))
				temp = new Lexeme (Constants.CONCAT, "Operator that concatenates a string");
			else if(token.Equals(Constants.STARTINIT))
				temp = new Lexeme(Constants.STARTINIT, "Assigns value after declaration.");
			else if(token.Equals(Constants.END_IF))
				temp = new Lexeme(Constants.END_IF, "Ends if");
			else if (token.Equals (Constants.NOTEQUAL))
				temp = new Lexeme (Constants.NOTEQUAL, "Operator for not equal comparison");
			else if (token.Equals (Constants.EXPCAST))
				temp = new Lexeme (Constants.EXPCAST, "Typecasts an expression");
			else if (token.Equals (Constants.A))
				temp = new Lexeme (Constants.A, "For " + Constants.EXPCAST);
			else if (token.Equals (Constants.YR))
				temp = new Lexeme (Constants.YR, "Separates the arguments");
			else if (token.Equals (Constants.INC))
				temp = new Lexeme (Constants.INC, "Increments a variable");
			else if (token.Equals (Constants.DEC))
				temp = new Lexeme (Constants.DEC, "Decrements a variable");
			else if (token.Equals (Constants.LOOPCONDFAIL))
				temp = new Lexeme (Constants.LOOPCONDFAIL, "Loops until FAIL");
			else if (token.Equals (Constants.LOOPCONDWIN))
				temp = new Lexeme (Constants.LOOPCONDWIN, "Loops until WIN");
			//else checks if the keyword matches any of the multi-word keywrds
			else if (token.StartsWith ("A")) {
				if (token.Equals (Constants.MANY_AND))
					temp = new Lexeme (Constants.MANY_AND, "AND Arity Operator");
				else if (token.Equals (Constants.MANY_OR))
					temp = new Lexeme (Constants.MANY_OR, "OR Arity Operator");
				else if(!token.EndsWith("OF") && token.Contains(" ")){ //if the keyword does not match the 2nd word or 3rd word... or 2nd to the last word and the token contain spaces
					string[] str = token.Split(delimiter); //splits the token into space

					temp = new Lexeme(str[0], Constants.VARDESC); //adds the first word to the list of lexeme as a variable name
					lex.Add(temp);

					temp = new Lexeme (); //resets the token

					token = str [1]; //gets the 2nd word as token
					checker (lex, c); //check if the 2nd word matches any keyword
					token = ""; //resets the token
				}
				else if(c == '\n') //if the token is already at the end of line
					temp = new Lexeme(token, Constants.VARDESC); //adds the token as a variable name
				else { //else adds the current character to token
					token += c;
					return;
				}
			}

			//same goes for the rest of these codes.

			else if (token.StartsWith ("B")) {
				if (token.Equals (Constants.MAX))
					temp = new Lexeme (Constants.MAX, "Operator that gets the bigger number");
				else if (token.Equals (Constants.AND))
					temp = new Lexeme (Constants.AND, "Logical Operator AND");
				else if (token.Equals (Constants.EQUAL))
					temp = new Lexeme (Constants.EQUAL, "Operator for equal comparison");
				else if((!token.EndsWith("OF") || !token.EndsWith("SAEM")) && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("D")) {
				if (token.Equals (Constants.SUB))
					temp = new Lexeme (Constants.SUB, "Operator that subtracts two numbers");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("E")) {
				if (token.Equals (Constants.OR))
					temp = new Lexeme (Constants.OR, "OR Logical Operator");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("F")) {
				if (token.Equals (Constants.RETURN))
					temp = new Lexeme (Constants.RETURN, "Returns value for functions");
				else if(!token.EndsWith("OF") && !token.EndsWith("YR") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("H")) {
				if (token.Equals (Constants.STARTFUNC))
					temp = new Lexeme (Constants.STARTFUNC, "Starting delimiter for functions");
				else if(!token.EndsWith("DUZ") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if(token.StartsWith("I")){
				if (token.Equals (Constants.VARDEC))
					temp = new Lexeme(Constants.VARDEC, "Declares a variable.");
				else if (token.Equals (Constants.VARCAST))
					temp = new Lexeme(Constants.VARCAST, "Typecasts a variable.");
				else if (token.Equals (Constants.ENDFUNC))
					temp = new Lexeme(Constants.ENDFUNC, "Ends the functions");
				else if (token.Equals (Constants.CALLFUNC))
					temp = new Lexeme(Constants.CALLFUNC, "Calls the functions");
				else if (token.Equals (Constants.STARTLOOP))
					temp = new Lexeme(Constants.STARTLOOP, "Starts the loop");
				else if (token.Equals (Constants.ENDLOOP))
					temp = new Lexeme(Constants.ENDLOOP, "Ends the loop");
				else if(!token.EndsWith("HAS") && 
					!token.EndsWith("NOW") && 
					!token.EndsWith("IN") && 
					!token.EndsWith("OUTTA") && 
					!token.EndsWith("IZ") && 
					!token.EndsWith("U") && 
					!token.EndsWith("SAY") && 
					token.Contains(" ")
				){
					string[] str = token.Split(delimiter, 2);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("M")) {
				if (token.Equals (Constants.MOD))
					temp = new Lexeme (Constants.MOD, "Operator that gets the modulo of two numbers");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("N")) {
				if (token.Equals (Constants.ELSE))
					temp = new Lexeme (Constants.ELSE, "Else statement");
				else if(!token.EndsWith("WAI") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("O")) {
				if (token.Equals (Constants.CONDITION))
					temp = new Lexeme (Constants.CONDITION, "Ends the condition");
				else if(!token.EndsWith("RLY?") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("P")) {
				if (token.Equals (Constants.MUL))
					temp = new Lexeme (Constants.MUL, "Operator that multiplies two numbers");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("Q")) {
				if (token.Equals (Constants.DIV))
					temp = new Lexeme (Constants.DIV, "Operator that divides two numbers");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("S")) {
				if (token.Equals (Constants.ADD))
					temp = new Lexeme (Constants.ADD, "Operator that adds two numbers");
				else if (token.Equals (Constants.MIN))
					temp = new Lexeme (Constants.MIN, "Operator that gets the smaller number");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("W")) {
				if (token.Equals (Constants.XOR))
					temp = new Lexeme (Constants.XOR, "Operator for XOR");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("Y")) {
				if (token.Equals (Constants.IF))
					temp = new Lexeme (Constants.IF, "If statement");
				else if(!token.EndsWith("RLY") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], Constants.VARDESC);
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, Constants.VARDESC);
				else {
					token += c;
					return;
				}
			}
			else if(token.Equals(Constants.ASSIGN))
				temp = new Lexeme(Constants.ASSIGN, "Assignment operation");
			else if(Constants.VARIDENT.IsMatch(token))
				temp = new Lexeme(token, Constants.VARDESC);
			else if(c!='\n')
				if(token.Length!=0) token += c;

			if (!temp.getName().Equals ("!")) { //adds the current lexeme to list of lexeme if the current lexeme is already initialized
				lex.Add(temp);
				token = "";
			}
		}
	}
}

