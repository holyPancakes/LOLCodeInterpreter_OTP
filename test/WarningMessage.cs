using System;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//messages that a syntax error will show
	public class WarningMessage
	{
		//when two tokens should be equal
		public static String shouldSame(string name1, string name2){
			return name1 + " and " + name2 + " should be the same!";
		}

		//when an unexpected lexeme is found
		public static String unexpectedLexeme(String name){
			if(name == Constants.EOL) return "Unexpected end of line found!";
			else return "Unexpected " + name + " found!";
		}

		//when a lexeme is expected in a line of code
		public static String expectedLexeme(String name){
			return "Expected " + name + " found!";
		}

		//when the ending double quote is not found
		public static String lackDoubleQuote(){
			return "String not properly closed!";
		}

		//when the program is not started properly
		public static String noStart(){
			return Constants.STARTPROG + " is not found to start the program!";
		}

		//when something went wrong in the right side of assignment operator R
		public static String RRightSide(){
			return "Only variable or constant or expression should be on the right side of " + Constants.ASSIGN;
		}

		//when something went wrong in the left side of assignment operator R
		public static String RLefttSide(){
			return "Only variable should be on the left side of " + Constants.ASSIGN;
		}

		//when the program already ended but there are still codes after
		public static String alreadyEnd(){
			return "Already ended the program with " + Constants.ENDPROG;
		}

		//when a variable is not declared
		public static String varNoDec(String varname){
			return "Variable Identifier " + varname + " is not yet declared!";
		}

		//when a variable is declared. (used when same variable name is declared)
		public static String varYesDec(String varname){
			return "Variable Identifier " + varname + " is already declared!";
		}

		//when a variable has no value
		public static String varNoVal(String varname){
			return "Variable Identifier " + varname + " has no value and can't be converted!";
		}

		//when a variable is impossible to be converted to a datatype
		public static String noConverto(String varname,String type){
			return "Variable Identifier " + varname + " can't be converted to " + type + "!";
		}

		//when a variable is not printable (keyword after VISIBLE, etc)
		public static String notPrintable(String toPrint){
			return toPrint + " is not printable!";
		}

		//when a keyword has no arguments
		public static String noArguments(String keyword){
			return keyword + " lacks or no arguments!";
		}

		//when a keyword has too many arguments
		public static String tooManyOperands(String keyword){
			return keyword + " has too many operands!";
		}

		//when something is expected after a keyword
		public static String expectedWord(String word1, String word2){
			return "Expected " + word1 + " after " + word2;
		}

		//when a certain operator lacks operands
		public static String lackOperands(String name){
			return "Lack of operands in " + name +"!";
		}

		//when an operator lacks operands
		public static String lackOperands(){
			return "Lack of operands!";
		}

		//when an operator cannot accept the keyword
		public static String canAccept(String keyword, String type){
			return keyword + "can only accept " + type + " datatype!";
		}

		//when an operator cannot convert the something to a certain datatype
		public static String cannotConvert(String name, String type){
			return "Cannot convert " + name + " to " + type + "!";
		}

		//when an MKAY is not found
		public static String noMKAY(){
			return Constants.MKAY + " not found in arity!";
		}

		//when the condition is not started with an IF
		public static String noIF(){
			return "Started Condition without " + Constants.IF;
		}

		//should be used for infinite loops. when a counter reached maximum value c# can hold
		public static String reachedMaxValue(){
			return "Loop continued indefinitely. Stopping because counter reached max value.";
		}

		//when a variable is not initialized and wanted to be printed
		public static String cannotNull(){
			return "Cannot print uninitialized variables!";
		}

		//when a certain escape character is not recognized
		public static String unrecognizedEscChar(String str){
			return "Cannot convert this " + str + "!";
		}
	}
}

