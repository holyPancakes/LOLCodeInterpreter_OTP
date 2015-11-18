using System;
using System.Text.RegularExpressions;

namespace test
{
	public class Constants
	{
		private static String varIdent = "^[A-Za-z][A-Za-z0-9_]*$"; //regex for variable
		private static String numbr = "^\\d+$"; //regex for numbr
		private static String numbar = "^(\\d+|\\d*\\.\\d+)$"; //regex for numbar
		private static String troof = "^(WIN|FAIL)$"; //regex for troof

		public static string PRINT = "VISIBLE"; //done
		public static string VARDEC = "I HAS A"; //done
		public static string ASSIGN = "R"; //done
		public static string ENDPROG = "KTHXBYE"; //done
		public static string STARTPROG = "HAI"; //done
		public static string STARTINIT = "ITZ"; //done

		public static string SCAN = "GIMMEH"; //
		public static string CONDITION = "O RLY?"; //
		public static string IF = "YA RLY"; //
		public static string ELSE = "NO WAI"; //
		public static string END_IF = "OIC";
		public static string SWITCH = "WTF?"; //
		public static string CASE = "OMG"; //
		public static string DEFAULT = "OMGWTF"; //
		public static string BREAK = "GTFO"; //

		public static string AN = "AN";//done
		public static string ADD = "SUM OF";//done
		public static string SUB = "DIFF OF"; //under
		public static string MUL = "PRODUKT OF"; //under
		public static string DIV = "QUOSHUNT OF";//under
		public static string MOD = "MOD OF";//under
		public static string MAX = "BIGGR OF";//under
		public static string MIN = "SMALLR OF";//under

		public static string AND = "BOTH OF";//
		public static string OR = "EITHER OF";//
		public static string XOR = "WON OF"; //
		public static string NOT = "NOT"; //
		public static string MANY_AND = "ALL OF"; //
		public static string MANY_OR = "ANY OF"; //

		public static String CONCAT = "SMOOSH";//done
		public static string MKAY = "MKAY"; //done

		public static string EQUAL = "BOTH SAEM";//
		public static string NOTEQUAL = "DIFFRINT OF"; //

		public static string ONELINE = "BTW";//done
		public static string MULTILINE = "OBTW"; //
		public static string ENDCOMMENT = "TLDR";//

		public static Regex VARIDENT = new Regex (varIdent, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex NUMBRVAL = new Regex (numbr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex NUMBARVAL = new Regex (numbar, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex TROOFVAL = new Regex (troof, RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
}

