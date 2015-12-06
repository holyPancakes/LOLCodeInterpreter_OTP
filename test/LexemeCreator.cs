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
		private Boolean oneLineComment;
		private String str;
		private String token;

		public LexemeCreator (){
			quotedDouble = false;
			isComment = false;
			oneLineComment = false;
			str = "";
			token = "";
		}

		public List<Lexeme> process(String[] lines){
			List<Lexeme> lex = new List<Lexeme>();

			foreach (String line in lines) {
				foreach (char c in line) {
					if (c == '\t' && !quotedDouble)
						continue;
					else if (c == '!' && !quotedDouble)
						lex.Add(new Lexeme(Constants.NONEWLINE, "Makes " + Constants.PRINT + " print with no new line."));
					else if ((c == ' ' || c == Constants.SOFTBREAKCHAR) && !quotedDouble) {
						if ((token.EndsWith (" ") || token.Length == 0) && c == ' ')
							continue;
						else if (token.Equals (Constants.ONELINE)) {
							if (!isComment) {
								Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment");
								oneLineComment = true;
								lex.Add (temp);
								token = "";
							}
						} else if (token.Equals (Constants.MULTILINE)) {
							Lexeme temp = new Lexeme (Constants.MULTILINE, "Multiline comment");
							lex.Add (temp);
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
						} else if (isComment || oneLineComment)
							token = "";
						else
							checker (lex, c); 
						if(c == Constants.SOFTBREAKCHAR)
							lex.Add(new Lexeme(Constants.SOFTBREAK, "Soft-command break"));
					} else if (c == '\"') {
						if (isComment || oneLineComment)
							continue;
						else if (str.EndsWith (":")) {
							str += c;
							continue;
						}
						quotedDouble = !quotedDouble;
						if (!quotedDouble) {
							Lexeme temp = new Lexeme (str, Constants.STRING + " constant"); //creates a lexeme
							lex.Add (temp);
							lex.Add (new Lexeme ("\"", "YARN Delimiter"));
							str = "";
						} else{
							lex.Add (new Lexeme ("\"", "YARN Delimiter"));
						}
					} else if (quotedDouble){
						str += c;
					} else{
						token += c;
					}
				}

				if (quotedDouble)
					throw new SyntaxException (WarningMessage.lackDoubleQuote ());
				if (token.Equals (Constants.ONELINE)) {
					Lexeme temp = new Lexeme (Constants.ONELINE, "One line comment");
					lex.Add (temp);
					isComment = true;
					token = "";
				} else if (token.Equals (Constants.MULTILINE)) {
					Lexeme temp = new Lexeme (Constants.MULTILINE, "Multiline comment");
					lex.Add (temp);
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
				} else if (isComment || oneLineComment)
					token = "";
				else
					checker (lex, '\n');

				if (token.Length != 0) {
					throw new SyntaxException (WarningMessage.unexpectedLexeme (token));
				}

				oneLineComment = false;
				lex.Add(new Lexeme(Constants.EOL, "Hard-command break"));
			}

			return lex;
		}

		private void checker(List<Lexeme> lex, char c){
			Lexeme temp = new Lexeme();
			char[] delimiter = { ' ' };

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
			else if (token.StartsWith ("A")) {
				if (token.Equals (Constants.MANY_AND))
					temp = new Lexeme (Constants.MANY_AND, "AND Arity Operator");
				else if (token.Equals (Constants.MANY_OR))
					temp = new Lexeme (Constants.MANY_OR, "OR Arity Operator");
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

			if (!temp.getName().Equals ("!")) {
				lex.Add(temp);
				token = "";
			}
		}
	}
}

