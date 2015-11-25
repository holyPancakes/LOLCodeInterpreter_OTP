using System;
using System.Text.RegularExpressions;

namespace test
{
	public static class Constants
	{
		private const String varIdent = "^[A-Za-z][A-Za-z0-9_]*$"; //regex for variable
		private const String numbr = "^\\d+$"; //regex for numbr
		private const String numbar = "^(\\d+|\\d*\\.\\d+)$"; //regex for numbar
		private const String troof = "^(WIN|FAIL)$"; //regex for troof

		public const string PRINT = "VISIBLE"; //done
		public const string VARDEC = "I HAS A"; //done
		public const string ASSIGN = "R"; //done
		public const string ENDPROG = "KTHXBYE"; //done
		public const string STARTPROG = "HAI"; //done
		public const string STARTINIT = "ITZ"; //done

		public const string SCAN = "GIMMEH"; //
		public const string CONDITION = "O RLY?"; //
		public const string IF = "YA RLY"; //
		public const string ELSE = "NO WAI"; //
		public const string END_IF = "OIC";
		public const string SWITCH = "WTF?"; //
		public const string CASE = "OMG"; //
		public const string DEFAULT = "OMGWTF"; //
		public const string BREAK = "GTFO"; //

		public const string AN = "AN";//done
		public const string ADD = "SUM OF";//done
		public const string SUB = "DIFF OF"; //under
		public const string MUL = "PRODUKT OF"; //under
		public const string DIV = "QUOSHUNT OF";//under
		public const string MOD = "MOD OF";//under
		public const string MAX = "BIGGR OF";//under
		public const string MIN = "SMALLR OF";//under

		public const string AND = "BOTH OF";//
		public const string OR = "EITHER OF";//
		public const string XOR = "WON OF"; //
		public const string NOT = "NOT"; //
		public const string MANY_AND = "ALL OF"; //
		public const string MANY_OR = "ANY OF"; //

		public const String CONCAT = "SMOOSH";//done
		public const string MKAY = "MKAY"; //done

		public const string EQUAL = "BOTH SAEM";//
		public const string NOTEQUAL = "DIFFRINT"; //

		public const string ONELINE = "BTW";//done
		public const string MULTILINE = "OBTW"; //
		public const string ENDCOMMENT = "TLDR";//

		public const String IMPLICITVAR = "IT";
		public const String EOL = @"\n";

		public static Regex VARIDENT = new Regex (varIdent, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex NUMBRVAL = new Regex (numbr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex NUMBARVAL = new Regex (numbar, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex TROOFVAL = new Regex (troof, RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
}