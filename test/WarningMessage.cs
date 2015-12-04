using System;

namespace test
{
	public class WarningMessage
	{
		public static String shouldSame(string name1, string name2){
			return name1 + " and " + name2 + " should be the same!";
		}

		public static String unexpectedLexeme(String name){
			return "Unexpected " + name + " found!";
		}

		public static String expectedLexeme(String name){
			return "Expected " + name + " found!";
		}

		public static String lackDoubleQuote(){
			return "String not properly closed!";
		}

		public static String noStart(){
			return Constants.STARTPROG + " is not found to start the program!";
		}

		public static String RRightSide(){
			return "Only variable or constant or expression should be on the right side of " + Constants.ASSIGN;
		}

		public static String RLefttSide(){
			return "Only variable should be on the left side of " + Constants.ASSIGN;
		}

		public static String alreadyEnd(){
			return "Already ended the program with " + Constants.ENDPROG;
		}

		public static String varNoDec(String varname){
			return "Variable Identifier " + varname + " is not yet declared!";
		}

		public static String varYesDec(String varname){
			return "Variable Identifier " + varname + " is already declared!";
		}

		public static String varNoVal(String varname){
			return "Variable Identifier " + varname + " has no value and can't be converted!";
		}
			
		public static String noConverto(String varname,String type){
			return "Variable Identifier " + varname + " can't be converted to " + type + "!";
		}

		public static String notPrintable(String toPrint){
			return toPrint + " is not printable!";
		}

		public static String noArguments(String keyword){
			return keyword + " lacks or no arguments!";
		}

		public static String tooManyOperands(String keyword){
			return keyword + " has too many operands!";
		}

		public static String expectedWord(String word1, String word2){
			return "Expected " + word1 + " after " + word2;
		}

		public static String lackOperands(String name){
			return "Lack of operands in " + name +"!";
		}

		public static String lackOperands(){
			return "Lack of operands!";
		}

		public static String canAccept(String keyword, String type){
			return keyword + "can only accept " + type + " datatype!";
		}

		public static String cannotConvert(String name){
			return "Cannot convert " + name + " to NUMBR or NUMBAR!";
		}

		public static String noMKAY(){
			return Constants.MKAY + " not found in arity!";
		}

		public static String noIF(){
			return "Started Condition without " + Constants.IF;
		}
	}
}

