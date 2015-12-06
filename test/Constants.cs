using System;
using System.Text.RegularExpressions;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//Contains all of the keywords and regex needed for this project.
	public static class Constants
	{
		private const String varIdent = "^[A-Za-z][A-Za-z0-9_]*$"; //pattern for variable identifier
		private const String numbr = "^-?\\d+$"; //pattern for int values
		private const String numbar = "^-?(\\d+|\\d*\\.\\d+)$"; //pattern for float values
		private const String troof = "^(WIN|FAIL)$"; //pattern for boolean values

		public const string PRINT = "VISIBLE"; //for printing
		public const string VARDEC = "I HAS A"; //for variable declaration
		public const string ASSIGN = "R"; //for assigning
		public const string ENDPROG = "KTHXBYE"; //for ending the program
		public const string STARTPROG = "HAI"; //for starting the program
		public const string STARTINIT = "ITZ"; //initializing the declared variable

		public const string SCAN = "GIMMEH"; //for scanning
		public const string CONDITION = "O RLY?"; //for starting the if-else statement
		public const string IF = "YA RLY"; //if statement
		public const string ELSE = "NO WAI"; //else statement
		public const string END_IF = "OIC";//ends the if-else statement
		public const string SWITCH = "WTF?"; //starts the switch-case
		public const string CASE = "OMG"; //for declaring a case in switch-case
		public const string DEFAULT = "OMGWTF"; //default case
		public const string BREAK = "GTFO"; //breaks out of loop and switch-case

		public const string A = "A";//used for typecasting especially the MAEK keyword
		public const string AN = "AN";//usually used for separating arguments

		public const string ADD = "SUM OF";//for adding two numbers
		public const string SUB = "DIFF OF"; //for subtracting two numbers
		public const string MUL = "PRODUKT OF"; //for multiplying two numbers
		public const string DIV = "QUOSHUNT OF";//for dividing two numbers
		public const string MOD = "MOD OF";//for getting the modulo two numbers
		public const string MAX = "BIGGR OF";//for getting the bigger number between two numbers
		public const string MIN = "SMALLR OF";//for getting the smaller number between two numbers

		public const string AND = "BOTH OF";//conjuncting two boolean value and checks if both are WIN
		public const string OR = "EITHER OF";//conjuncting two boolean value and checks if at least one is WIN
		public const string XOR = "WON OF"; //conjuncting two boolean value and checks if both are different and at least one is WIN
		public const string NOT = "NOT"; //negates the boolean value
		public const string MANY_AND = "ALL OF"; //AND arity
		public const string MANY_OR = "ANY OF"; //OR Arity

		public const String CONCAT = "SMOOSH";//concatenating values
		public const string MKAY = "MKAY"; //ends arity except VISIBLE

		public const string EQUAL = "BOTH SAEM";//checks if the values are equal
		public const string NOTEQUAL = "DIFFRINT"; //checks if the values are not equal

		public const string ONELINE = "BTW";//one line comment
		public const string MULTILINE = "OBTW"; //starts the multi-line comment
		public const string ENDCOMMENT = "TLDR";//ends the multi-line comment

		public const String IMPLICITVAR = "IT"; //implicit variable

		public const String EOL = @"\n"; //hard breaks the command
		public const char SOFTBREAKCHAR = ','; //soft breaks the command (char datatype)
		public const String SOFTBREAK = ","; //soft breaks the command (string datatype)

		public const String VARDESC = "Variable Identifier"; //lexeme description for a variable identifier

		public const String VARCAST = "IS NOW A"; //changes a variable datatype
		public const String EXPCAST = "MAEK"; //returns the value with a new datatype

		public const String STARTFUNC = "HOW IZ I"; //starts the function declaration
		public const String ENDFUNC = "IF U SAY SO"; //ends the function body
		public const String RETURN = "FOUND YR"; //returns the value in function
		public const String CALLFUNC = "I IZ"; //calls the function

		public const String NULL = "NOOB"; //default 'value' when a variable is not declared
		public const String NOTYPE = "Untyped"; //default datatype when a variable is not declared

		public const String INT = "NUMBR"; //int datatype
		public const String FLOAT = "NUMBAR"; //float datatype
		public const String STRING = "YARN"; //string datatype
		public const String BOOL = "TROOF"; //boolean datatype

		public const String STARTLOOP = "IM IN YR"; //starts the loop
		public const String ENDLOOP = "IM OUTTA YR"; //ends the loop
		public const String YR = "YR"; //shows which variable will be used as counter
		public const String INC = "UPPIN"; //increments the counter
		public const String DEC = "NERFIN"; //decrements the counter
		public const String LOOPCONDFAIL = "TIL"; //loops until it wins
		public const String LOOPCONDWIN = "WILE"; //loops until it fails

		public const String TRUE = "WIN"; //true value for boolean
		public const String FALSE = "FAIL"; //fail value for boolean

		public const String NONEWLINE = "!"; //states that VISIBLE will not add new line after the statement

		public static Regex VARIDENT = new Regex (varIdent, RegexOptions.Compiled); //regex for variable identifieir
		public static Regex INTVAL = new Regex (numbr, RegexOptions.Compiled); //regex for integer value
		public static Regex FLOATVAL = new Regex (numbar, RegexOptions.Compiled); //regex for float value
		public static Regex BOOLVAL = new Regex (troof, RegexOptions.Compiled); //regex for boolean value
	}
}