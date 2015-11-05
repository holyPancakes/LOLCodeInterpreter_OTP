using System;
using System.IO;
using Gtk;
using System.Collections.Generic;

namespace test
{
	public class Interpreter
	{
		LexemeCreator lexer = new LexemeCreator(); //for lexical analysis
		List<Lexeme> lexemesList = new List<Lexeme>();
		Parser parser = new Parser(); //parser (syntax analysis)
		String line;


		public Interpreter (){
			Interpret();
		}
		public void Interpret (String sourceText){
			char[] delimeter = {'\n'};
			string[] sourceLines = sourceText.Split (delimeter);
			foreach(string line in sourceLines){ //infinite loop
				if(line.Equals("KTHXBYE"))
				{
					break; //if quit is typed, closes the program
				}
				try{
					lexemesList = lexer.process(line); //creates an array of lexemes
					parser.process(lexeme, false); //parses the lexemes
				}catch(Exception e){ //if something went wrong, prints the error on screen

				}
				lexer.reset(); //resets the lexer
			}
		
		}
		
	
	}
}

