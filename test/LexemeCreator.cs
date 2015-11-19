using System;
using GLib;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace test
{
	public class LexemeCreator
	{
		private Boolean quotedDouble;
		private Boolean isComment;
		private String str;
		private String token;

		public LexemeCreator ()
		{
			quotedDouble = false;
			isComment = false;
		}

		public List<Lexeme> process(String line)
		{ //processes a string
			
			List<Lexeme> lex = new List<Lexeme>();

			foreach(char c in line) { //converts it to array of characters
				//Console.WriteLine(token);
				if(c == '\t')
					continue; //ignore if it is a tab
				else if(c == ' ' && !quotedDouble) { //checks if the character is space and not quoted
					if (token.EndsWith (" ") || token.Length == 0)
						continue; //ignore if it is a space
					else if (token.Equals (Constants.ONELINE)) {
						Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment");
						lex.Add(temp);
						token = "";
						return lex;
					} else if (token.Equals (Constants.MULTILINE)) {
						Lexeme temp = new Lexeme(Constants.MULTILINE, "Multiline comment");
						lex.Add(temp);
						isComment = true;
						token = "";
					} else if (token.Equals (Constants.ENDCOMMENT)) {
						if (isComment) {
							Lexeme temp = new Lexeme (Constants.ENDCOMMENT, "Ends the comment");
							lex.Add (temp);
							isComment = false;
							token = "";
						} else
							throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDCOMMENT));
					}else if (isComment) token = "";
					else checker(lex, c); //else checks the token if it is a keyword or a value
				} else if(c == '\"') { //checks if the char is a double quotes
					if(isComment) continue;
					quotedDouble = !quotedDouble; //switches the flags of double quotes
					if (!quotedDouble) { //if the double quote becomes false
						Lexeme temp = new Lexeme (str, "YARN constant"); //creates a lexeme
						lex.Add (temp); //adds the lexeme to an array of lexemes
						lex.Add (new Lexeme ("\"", "YARN Delimiter"));
						str = ""; //resets the string
					} else
						lex.Add(new Lexeme("\"", "YARN Delimiter"));
				}
				else if(quotedDouble)
					str += c; //append the char to string
				else
					token+=c;//appends the char to token otherwise
			}
			//Console.WriteLine(token);

			if(quotedDouble) //checks if the string is properly closed
				throw new SyntaxException (WarningMessage.lackDoubleQuote());
			if (token.Equals (Constants.ONELINE)) {
				Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment");
				lex.Add(temp);
				token = "";
				return lex;
			} else if (token.Equals (Constants.MULTILINE)) {
				Lexeme temp = new Lexeme(Constants.MULTILINE, "Multiline comment");
				lex.Add(temp);
				isComment = true;
				token = "";
			} else if (token.Equals (Constants.ENDCOMMENT)) {
				if (isComment) {
					Lexeme temp = new Lexeme (Constants.ENDCOMMENT, "Ends the comment");
					lex.Add (temp);
					isComment = false;
					token = "";
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDCOMMENT));
			}else if (isComment) token = "";
			else checker(lex, '\n'); //checks the token of it is not yet empty

			if(token.Length!=0)
			{ //checks if the token is not a keyword and found dangling in the code
				throw new SyntaxException(WarningMessage.unexpectedLexeme (token));
			}

			return lex; //returns the array of lexemes
		}

		private void checker(List<Lexeme> lex, char c)
		{ //checks the token if it is a keyword or a value
			Lexeme temp = new Lexeme();
			char[] delimiter = { ' ' };

			if (Constants.NUMBRVAL.IsMatch (token))
				temp = new Lexeme (token, "NUMBR constant");
			else if (Constants.NUMBARVAL.IsMatch (token))
				temp = new Lexeme (token, "NUMBAR constant");
			else if (Constants.TROOFVAL.IsMatch (token))
				temp = new Lexeme (token, "TROOF constant");
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
				temp = new Lexeme (Constants.NOT, "Operator that negates the condition");
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
			else if (token.StartsWith ("A")) {
				if (token.Equals (Constants.MANY_AND))
					temp = new Lexeme (Constants.MANY_AND, "AND Arity Operator");
				else if (token.Equals (Constants.MANY_OR))
					temp = new Lexeme (Constants.MANY_OR, "OR Arity Operator");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
				else {
					token += c;
					return;
				}
			}

			else if (token.StartsWith ("B")) {
				if (token.Equals (Constants.MAX))
					temp = new Lexeme (Constants.MAX, "Operator that gets the bigger number");
				else if (token.Equals (Constants.AND))
					temp = new Lexeme (Constants.AND, "Logical Operator and");
				else if (token.Equals (Constants.EQUAL))
					temp = new Lexeme (Constants.EQUAL, "Operator for equal comparison");
				else if((!token.EndsWith("OF") || !token.EndsWith("SAEM")) && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
				else {
					token += c;
					return;
				}
			}
			else if(token.StartsWith("I")){ //checks if it is possibly a I HAS A keyword
				if (token.Equals (Constants.VARDEC))
					temp = new Lexeme(Constants.VARDEC, "Declares a variable.");
				else if(!token.EndsWith("HAS") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
				else {
					token += c;
					return;
				}
			}
			else if (token.StartsWith ("S")) {
				if (token.Equals (Constants.ADD))
					temp = new Lexeme (Constants.ADD, "Operator that adds two numbers");
				else if (token.Equals (Constants.MIN))
					temp = new Lexeme (Constants.MIN, "Gets the smaller number");
				else if(!token.EndsWith("OF") && token.Contains(" ")){
					string[] str = token.Split(delimiter);
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
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
					temp = new Lexeme(str[0], "Variable Identifier");
					lex.Add(temp);
					temp = new Lexeme ();
					token = str [1];
					checker (lex, c);
					token = "";
				}
				else if(c == '\n')
					temp = new Lexeme(token, "Variable Identifier");
				else {
					token += c;
					return;
				}
			}
			else if(token.Equals(Constants.ASSIGN))
				temp = new Lexeme(Constants.ASSIGN, "Assignment operation");
			else if(Constants.VARIDENT.IsMatch(token))
				temp = new Lexeme(token, "Variable Identifier");
			else if(c!='\n')
				if(token.Length!=0) token += c;

			if (!temp.getName().Equals ("!")) {
				lex.Add(temp);
				token = "";
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

