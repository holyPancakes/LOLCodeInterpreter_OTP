using System;
using Gtk;
using test;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;


public partial class MainWindow: Gtk.Window
{	
	private LexemeCreator lexer = new LexemeCreator(); //for lexical analysis
	private List<Lexeme> lexemeList = new List<Lexeme>();
	private Stack<Value> stack = new Stack<Value> ();
	private Stack<Value> stackArity = new Stack<Value>();
	private char[] delimeter = {'\n'};

	private Dictionary<String, Value> table = new Dictionary<String, Value>(); //symbol table
	private Boolean hasEnded; //checks if the program already ended using KTHXBYE
	private Boolean hasStarted; //checks if the program already started using HAI
	private Boolean hasError;
	private Gtk.ListStore tokensListStore;
	private Gtk.ListStore symbolTableListStore;

	TextView inputTextView;

	public MainWindow (): base (Gtk.WindowType.Toplevel){
		Build ();

		Gtk.TreeViewColumn lexemeColumn = new Gtk.TreeViewColumn ();
		lexemeColumn.Title = "Lexeme";
		Gtk.CellRendererText lexemeCell = new Gtk.CellRendererText ();
		lexemeColumn.PackStart (lexemeCell, true);

		Gtk.TreeViewColumn lexemeDescriptionColumn = new Gtk.TreeViewColumn ();
		lexemeDescriptionColumn.Title = "Description";
		Gtk.CellRendererText lexemeDescriptionCell = new Gtk.CellRendererText ();
		lexemeDescriptionColumn.PackStart (lexemeDescriptionCell, true);

		tokensTreeView.AppendColumn (lexemeColumn);
		tokensTreeView.AppendColumn (lexemeDescriptionColumn);

		lexemeColumn.AddAttribute (lexemeCell, "text", 0);
		lexemeDescriptionColumn.AddAttribute (lexemeDescriptionCell, "text", 1);

		tokensListStore = new Gtk.ListStore (typeof (string), typeof (string));

		tokensTreeView.Model = tokensListStore;


		Gtk.TreeViewColumn varNameColumn = new Gtk.TreeViewColumn ();
		varNameColumn.Title = "Variable Name";
		Gtk.CellRendererText varNameCell = new Gtk.CellRendererText ();
		varNameColumn.PackStart (varNameCell, true);

		Gtk.TreeViewColumn valueColumn = new Gtk.TreeViewColumn ();
		valueColumn.Title = "Value";
		Gtk.CellRendererText valueCell = new Gtk.CellRendererText ();
		valueColumn.PackStart (valueCell, true);

		Gtk.TreeViewColumn typeColumn = new Gtk.TreeViewColumn ();
		typeColumn.Title = "Data Type";
		Gtk.CellRendererText typeCell = new Gtk.CellRendererText ();
		typeColumn.PackStart (typeCell, true);

		symbolTableTreeView.AppendColumn (varNameColumn);
		symbolTableTreeView.AppendColumn (valueColumn);
		symbolTableTreeView.AppendColumn (typeColumn);

		varNameColumn.AddAttribute (varNameCell, "text", 0);
		valueColumn.AddAttribute (valueCell, "text", 1);
		typeColumn.AddAttribute (typeCell, "text", 2);

		symbolTableListStore = new Gtk.ListStore (typeof (string), typeof (string), typeof (string));

		symbolTableTreeView.Model = symbolTableListStore;

		sourceText.Buffer.Text = Constants.STARTPROG + "\n\n" + Constants.ENDPROG;
		sourceText.GrabFocus ();
	}

	public void Interpret(){
		outputField.Buffer.Text = "";

		hasStarted = false;
		hasEnded = false;
		hasError = false;

		tokensListStore.Clear ();
		symbolTableListStore.Clear ();
		lexemeList.Clear ();
		table.Clear ();
		stack.Clear ();
		stackArity.Clear ();
		lexer.reset ();

		table.Add (Constants.IMPLICITVAR, new Value ("NOOB", "Untyped"));

		string sourceString = sourceText.Buffer.Text;
		string[] sourceLines = sourceString.Split (delimeter);
		int i = 0;
		try {
			lineField.Buffer.Text = "Creating Lexemes...";
			lexemeList = lexer.process (sourceLines, ref i); //creates an array of lexemes
			i=0;
			UpdateLexemes(lexemeList);
			lineField.Buffer.Text = "Running the Lexemes...";
			parse (ref i); //parses the lexemes
		} catch (SyntaxException e) { //if something went wrong, prints the error on screen
			hasError = true;
			outputField.Buffer.Text += "\n" + "ERROR on line " + (i+1) + ": " + e.Message+ "\n";
			return;
		} catch (WarningException e){
			outputField.Buffer.Text += "\n" + "WARNING on line " + (i+1) + ": " + e.Message+ "\n";
			return;
		} catch (Exception e){
			outputField.Buffer.Text += "\n" + "CRASHED on line " + (i+1) + ": " + e.Message + "\n" + e.StackTrace+ "\n";
			return;
		}
		if(!hasEnded && !hasError) outputField.Buffer.Text += "\n" + "ERROR on line " + (sourceLines.Length+1) + ": Program is not closed properly!\n";
		if(hasEnded) lineField.Buffer.Text = "Done!";

		Console.WriteLine ("----------");
	}

	public void parse(ref int lineNo)
	{ //processses the lexemes
		//Console.WriteLine("\nPARSING:");
		bool nextCommand = false;

		if (lexemeList.Count == 0)
			return;

		for(int i=0; i < lexemeList.Count; i++){
			Console.WriteLine (lexemeList[i].getName());
			if (lexemeList [i].getDescription ().Contains ("comment"))
				continue;
			else if (lexemeList [i].getName () == Constants.EOL) {
				nextCommand = false;
				lineNo++;
			} else if (nextCommand) {
				throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i].getName ()));
			} else if (lexemeList [i].getName ().Equals (Constants.STARTPROG)) {
				if(hasStarted) 
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.STARTPROG));
				hasStarted = true;
				nextCommand = true;
			} else if (!hasStarted)
				throw new SyntaxException (WarningMessage.noStart());
			else if(hasEnded)
				throw new SyntaxException(WarningMessage.alreadyEnd());
			else{
				if (lexemeList [i].getName ().Equals (Constants.PRINT)) { //checks if the keyword is VISIBLE
					printParse(ref i);
				} else if (lexemeList [i].getName ().Equals (Constants.SCAN)) {
					scanParse (ref i);
				} else if (lexemeList [i].getName ().Equals (Constants.VARDEC)) { //checks if the keyword is I HAS A
					varDecParse(ref i);
				} else if (lexemeList [i].getName ().Equals (Constants.ASSIGN)) { //checks if the keyword is R
					assignParse(ref i);
				} else if (lexemeList [i].getDescription ().Equals ("Variable Identifier")) { //checks if the keyword is KTHXBYE
					if (!table.ContainsKey (lexemeList [i].getName ())) //check if the variable is already declared
						throw new SyntaxException (WarningMessage.varNoDec (lexemeList [i].getName ()));
					else if (lexemeList [i + 1].getName () == Constants.ASSIGN)
						continue;
					else { 
						table [Constants.IMPLICITVAR] = table [lexemeList [i].getName ()];
					}
				} else if (lexemeList [i].getName ().Equals (Constants.ENDPROG)) { //checks if the keyword is KTHXBYE
					hasEnded = true;
				} else if (lexemeList [i].getDescription ().Contains ("Operator")) {
					string value = operatorList (lexemeList, ref i);
					string type = "";
					if (Constants.NUMBRVAL.Match (value).Success)
						type = "NUMBR";
					else if (Constants.NUMBARVAL.Match (value).Success)
						type = "NUMBAR";
					else if (Constants.TROOFVAL.Match (value).Success)
						type = "TROOF";
					else
						type = "YARN";

					table[Constants.IMPLICITVAR] = new Value (value, type);
				} else
					throw new WarningException(WarningMessage.unexpectedLexeme(lexemeList[i].getName()));
				nextCommand = true;
			}
			UpdateTables();
		}
		//Console.WriteLine("========");
	}

	private void printParse(ref int i){
		if (lexemeList.Count - i >= 2) { //checks if VISIBLE has arguments
			Lexeme l = lexemeList [i + 1]; //gets the next lexeme
			String toWrite;
			if (l.getName ().Equals ("\"")) { //checks if it is a string constant
				Lexeme yarn = lexemeList [i + 2];
				toWrite = yarn.getName () + "\n"; //prints the string
				i += 3;
			} else if (l.getDescription ().Equals ("Variable Identifier")) {
				if (!table.ContainsKey (l.getName ())) //checks if the variable is already declared
					throw new SyntaxException (WarningMessage.varNoDec (l.getName ()));
				Value val = table [l.getName ()]; //gets the value of the variable
				toWrite = val.getValue () + "\n"; //prints the value of the variable
				i++;
			} else if (l.getDescription ().Contains ("Operator")) {
				i++;
				toWrite = operatorList (lexemeList, ref i);
				i--;
			} else
				throw new SyntaxException (WarningMessage.notPrintable (l.getName ()));
			outputField.Buffer.Text += toWrite;
		} else //else VISIBLE has no arguments
			throw new SyntaxException (WarningMessage.noArguments (Constants.PRINT));
	}

	private void scanParse(ref int i){
		if (lexemeList.Count - i >= 2) {
			if (table.ContainsKey (lexemeList [i + 1].getName ())) {
				Dialog inputPrompt = null;
				ResponseType response = ResponseType.None;

				try {
					inputPrompt = new Dialog ("GIMMEH", this, 
						DialogFlags.DestroyWithParent | DialogFlags.Modal, 
						"OK", ResponseType.Yes);

					inputPrompt.Resize (300, 50);
					inputPrompt.VBox.Add (inputTextView = new TextView ());
					inputPrompt.ShowAll ();

					response = (ResponseType)inputPrompt.Run ();
				} finally {
					if (inputPrompt != null) {
						if (response == ResponseType.Yes) {
							string input = inputTextView.Buffer.Text;
							Value val = new Value (input, "YARN");
							table [lexemeList [i + 1].getName ()] = val; 
						}
						inputPrompt.Destroy ();
					}
				}
			} else
				throw new SyntaxException (WarningMessage.varNoDec (lexemeList [i + 1].getName ()));
		} else
			throw new SyntaxException (WarningMessage.noArguments (Constants.SCAN));
		i++;
	}

	private void varDecParse(ref int i){
		char[] spaces = { ' ' };
		if (lexemeList.Count - i == 2) { //checks if I HAS A has arguments
			Lexeme l = lexemeList [i + 1]; //gets the next lexeme
			if (l.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
				if (table.ContainsKey (l.getName ())) //checks if the variable is already in the table
					throw new SyntaxException (WarningMessage.varYesDec (l.getName ()));
				table.Add (l.getName (), new Value ("NOOB", "Untyped")); //makes the variable NOOB
			} else
				throw new SyntaxException (WarningMessage.expectedWord ("variable declaration", Constants.VARDEC));
			i++; //increments the index
		} else if (lexemeList.Count > 2) { //checks if the statement also starts with a value
			if (lexemeList [i + 2].getDescription ().Contains ("comment")) {
				Lexeme l = lexemeList [i + 1]; //gets the next lexeme
				if (l.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
					if (table.ContainsKey (l.getName ())) //checks if the variable is already in the table
						throw new SyntaxException (WarningMessage.varYesDec (l.getName ()));
					table.Add (l.getName (), new Value ("NOOB", "Untyped")); //makes the variable NOOB
				} else
					throw new SyntaxException (WarningMessage.expectedWord ("variable declaration", Constants.VARDEC));
				i++; //increments the index
			} else {
				Lexeme l1 = lexemeList [i + 1]; //gets the next lexeme
				if (l1.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
					if (table.ContainsKey (l1.getName ())) //checks if the variable is in the table
						throw new SyntaxException (WarningMessage.varYesDec (l1.getName ()));
					Lexeme l2 = lexemeList [i + 2]; //gets the next lexeme of next lexeme
					if (l2.getName ().Equals (Constants.STARTINIT)) { //checks if it is ITZ
						Lexeme l3 = lexemeList [i + 3]; //gets the argument of ITZ
						if (l3.getDescription ().EndsWith ("constant")) { //checks if the argument is contstant
							String[] type = l3.getDescription ().Split (spaces); //gets the datatype of the value
							table.Add (l1.getName (), new Value (l3.getName (), type [0])); //puts it to table
						} else if (l3.getName ().Equals ("\"")) { //else ITZ has no arguments
							Lexeme l4 = lexemeList [i + 4];
							String[] type = l4.getDescription ().Split (spaces); //gets the datatype of the value
							table.Add (l1.getName (), new Value (l4.getName (), type [0])); //puts it to table
							i += 2;
						} else if (table.ContainsKey (l3.getName ())) { //checks if the argument is a variable and it is initialized
							table.Add (l1.getName (), table [l3.getName ()]); //copies the value and puts it to the table
						} else if (l3.getDescription ().Contains ("Operator")) {
							i += 3;
							string val = operatorList (lexemeList, ref i);
							string type = "";
							if (Constants.NUMBRVAL.Match (val).Success)
								type = "NUMBR";
							else if (Constants.NUMBARVAL.Match (val).Success)
								type = "NUMBAR";
							else if (Constants.TROOFVAL.Match (val).Success)
								type = "TROOF";
							else
								type = "YARN";

							table.Add (l1.getName (), new Value (val, type));
						} else
							throw new SyntaxException (WarningMessage.expectedWord ("constant or variable", Constants.STARTINIT));
					} else
						throw new SyntaxException (WarningMessage.expectedWord (Constants.STARTINIT, "variable declaration"));
				} else
					throw new SyntaxException (WarningMessage.expectedWord ("variable declaration", Constants.STARTINIT));
			}
			i += 3;
		} else
			throw new SyntaxException (WarningMessage.noArguments (Constants.VARDEC));
	}

	private void assignParse(ref int i){
		Lexeme var = lexemeList [i - 1]; //gets the left and right lexemes of R
		Lexeme value = lexemeList [i + 1];
		char[] spaces = { ' ' };

		if (var.getDescription ().Equals ("Variable Identifier")) { //checks if the left side is a variable
			if (value.getDescription ().EndsWith ("constant")) { //checks if the right side is a constant
				String[] type = value.getDescription ().Split (spaces); //gets the dataype
				Value old = table [var.getName ()]; //gets the old datatype of the variable

				table [var.getName ()] = new Value (value.getName (), type [0]); //puts new value to table
			} else if(value.getName().Equals("\"")){
				Lexeme yarn = lexemeList [i + 2];
				String[] type = yarn.getDescription ().Split (spaces); //gets the dataype
				Value old = table [var.getName ()]; //gets the old datatype of the variable

				table [var.getName ()] = new Value (yarn.getName (), type [0]); //puts new value to table
				i+=2;
			}else if (value.getDescription ().Equals ("Variable Identifier")) { //checks if the right side is a variable
				if (!table.ContainsKey (value.getName ())) //check if the variable is already declared
					throw new SyntaxException (WarningMessage.varNoDec (value.getName ()));

				table[var.getName ()] = table [value.getName ()]; //changes the value of the variable
			} else if (value.getDescription ().Contains ("Operator")) {
				i++;
				string val = operatorList (lexemeList, ref i);
				string type = "";
				if (Constants.NUMBRVAL.Match (val).Success)
					type = "NUMBR";
				else if (Constants.NUMBARVAL.Match (val).Success)
					type = "NUMBAR";
				else if (Constants.TROOFVAL.Match (val).Success)
					type = "TROOF";
				else
					type = "YARN";

				table [var.getName ()] = new Value (val, type);
			} else //else the right side is neither a variable or a constant
				throw new SyntaxException (WarningMessage.RRightSide ());

		} else //else the left side is not a variable
			throw new SyntaxException (WarningMessage.RLefttSide ());

		i++;
	}

	private String operatorList(List<Lexeme> lexemeList, ref int index){
		if (index + 1 == lexemeList.Count)
			throw new SyntaxException (WarningMessage.noArguments(lexemeList[index].getName()));

		string name = lexemeList [index].getName ();
		string dec = lexemeList [index].getDescription ();
		string result = "";

		if (name.Equals (Constants.CONCAT)) {
			index++;
			result = concatString (lexemeList, ref index);
		} else if (dec.Contains("Operator") && !dec.Contains("Arity")){
			result = mathOperation (lexemeList, ref index);
		} else if(dec.Contains("Arity")) {
			result = arityOperation(lexemeList, ref index);
		} else
			throw new WarningException(WarningMessage.unexpectedLexeme(name));
		
		return result;
	}

	private String mathOperation(List<Lexeme> lexemeList, ref int index){
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";
		for (; lexemeList[index].getName() != Constants.EOL; index++) {
			String name = lexemeList [index].getName ();
			String desc = lexemeList [index].getDescription ();

			if (desc.Contains ("comment")) {
				continue;
			} else if (name == Constants.AN) {
				if (ANned)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else
					ANned = true;
			} else if (Constants.NUMBRVAL.Match (name).Success ||
			           Constants.NUMBARVAL.Match (name).Success ||
			           Constants.TROOFVAL.Match (name).Success) {
				if (ANned) {
					String[] type = desc.Split (delimiter);
					stack.Push (new Value (name, type [0]));
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			} else if (desc == "Variable Identifier"){
				if (table.ContainsKey (name)) {
					Value val = table [name];
					stack.Push (val);
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.varNoDec (name));
			} else {
				stack.Push (new Value (name, "Operator"));
				ANned = true;
			}
		}

		result = evaluateCond ();
		Console.WriteLine ("Result in math op: " + result);
		return result;
	}

	private String arityOperation(List<Lexeme> lexemeList, ref int index){
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";
		for (; lexemeList[index].getName() != Constants.EOL; index++) {
			String name = lexemeList [index].getName ();
			String desc = lexemeList [index].getDescription ();

			if (desc.Contains ("comment")) {
				continue;
			} else if (name == Constants.AN) {
				if (ANned)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else
					ANned = true;
			} else if (Constants.NUMBRVAL.Match (name).Success ||
				Constants.NUMBARVAL.Match (name).Success ||
				Constants.TROOFVAL.Match (name).Success) {
				if (ANned) {
					String[] type = desc.Split (delimiter);
					stackArity.Push (new Value (name, type [0]));
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			} else if (desc == "Variable Identifier"){
				if (table.ContainsKey (name)) {
					Value val = table [name];
					stackArity.Push (val);
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.varNoDec (name));
			} else {
				stackArity.Push (new Value (name, "Operator"));
				ANned = true;
			}
		}

		result = evaluateArity ();
		return result;
	}

	private String evaluateArity(){
		String result = "";
		Value popped;
		List<Value> values = new List<Value>();
		Stack<Value> temp = new Stack<Value> ();
		bool start = true;

		while (stackArity.Count > 1) {
			Console.Write ("Stack Arity Stack: ");
			foreach (Value v in stackArity) {
				Console.Write (v.getValue () + " ");
			}
			Console.WriteLine ();

			Console.Write ("Values List: ");
			foreach (Value v in values) {
				Console.Write (v.getValue () + " ");
			}
			Console.WriteLine ();

			popped = stackArity.Pop();
			Console.WriteLine ("Popped: " + popped.getValue ());

			if (popped.getValue () == Constants.MKAY) {
				if (!start)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (popped.getValue ()));
				else {
					start = false;
					continue;
				}
			} else if (stackArity.Peek ().getValue () == Constants.NOT) {
				stackArity.Pop ();
				values.Add(new Value((popped.getValue() == "WIN")? "FAIL": "WIN", "TROOF"));
			} else if (popped.getType () != "TROOF") {
				temp.Push (popped);
				while ((popped = stackArity.Pop ()).getType () != "Operator") {
					temp.Push (popped);
				}
				temp.Push (popped);

				while (temp.Count > 0) {
					stack.Push (temp.Pop ());
				}
				result = evaluateCond ();
				if (stackArity.Peek ().getValue () == Constants.NOT){
					stackArity.Pop ();
					result = (result == "WIN") ? "FAIL" : "WIN";
				}
				values.Add (new Value (result, "TROOF"));
			} else {
				values.Add (popped);
			}

			start = false;
		}

		Console.Write ("Values List: ");
		foreach (Value v in values) {
			Console.Write (v.getValue () + " ");
		}
		Console.WriteLine ();
		
		popped = stackArity.Pop ();
		if (popped.getValue () == Constants.MANY_AND) {
			foreach (Value v in values) {
				if (v.getValue () == "FAIL") {
					result = "FAIL";
					break;
				}
			}
			if (result != "FAIL")
				result = "WIN";
		} else if (popped.getValue () == Constants.MANY_OR) {
			foreach (Value v in values) {
				if (v.getValue () == "WIN") {
					result = "WIN";
				}
			}
			if (result != "WIN")
				result = "FAIL";
		} else
			throw new SyntaxException(WarningMessage.unexpectedLexeme(popped.getValue()));
		Console.WriteLine ("Result in Arity Evaluation: " + result);
		return result;
	}

	private String evaluateCond (){
		Value val1 = new Value("NOOB", "Untyped");
		Value val2 = new Value("NOOB", "Untyped");
		Value op = null;
		Value temp = null;
		String result = "";
		bool overPop = false;

		while(stack.Count > 1){
			Console.Write ("Stack stack: ");
			foreach (Value v in stack) {
				Console.Write (v.getValue() + " ");
			}
			Console.WriteLine ();

			try{
				val2 = stack.Pop();
				if(stack.Count > 1){
					if(stack.Peek().getValue() == Constants.NOT){
						if(val2.getType() != "TROOF")
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val2.getValue()));
						else{
							stack.Pop();
							foreach (Value v in stack) {
								Console.Write (v.getValue() + " ");
							}
							Console.WriteLine ();
							val2 = new Value((val2.getValue() == "WIN")? "FAIL": "WIN", "TROOF");
						}
					}
				}
				if(stack.Count > 1){
					val1 = stack.Pop();
					if(stack.Peek().getValue() == Constants.NOT){
						if(val2.getType() != "TROOF")
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val2.getValue()));
						else{
							stack.Pop();
							foreach (Value v in stack) {
								Console.Write (v.getValue() + " ");
							}
							Console.WriteLine ();
							val2 = new Value((val2.getValue() == "WIN")? "FAIL": "WIN", "TROOF");
						}
					}
				}
				op = stack.Pop();
				Console.WriteLine ("Types: " + val1.getType() + " " + val2.getType() + " " + op.getType());
				if(op.getType() != "Operator"){
					overPop = true;

					temp = val2;
					val2 = val1;
					val1 = op;
					op = stack.Pop();
					Console.WriteLine ("Types: " + val1.getType() + " " + val2.getType() + " " + op.getType());
				}
			} catch(Exception){
				throw new SyntaxException (WarningMessage.lackOperands() + " Resulted in stack underflow.");
			}

			String str1 = val1.getValue ();
			String str2 = val2.getValue ();
			String type1 = val1.getType ();
			String type2 = val2.getType ();
			String type = "";

			float f1 = float.MinValue;
			float f2 = float.MinValue;
			bool b1 = false;
			bool b2 = false;

			if (type1 == "TROOF" || type2 == "TROOF") {
				if (type1 == "TROOF") {
					b1 = (str1 == "WIN") ? true : false;
					if (type2 == "TROOF")
						b2 = (str2 == "WIN") ? true : false;
					else
						f2 = float.Parse (str2);
					type = "TROOF";
				} else if (type2 == "TROOF") {
					b2 = (str2 == "WIN") ? true : false;
					if(type1 != "Untyped") f1 = float.Parse (str1);
					type = "TROOF";
				}else {
					f1 = float.Parse (str1);
					f2 = float.Parse (str2);
					type = (val1.getType () == "NUMBAR" || val2.getType () == "NUMBAR") ? "NUMBAR" : "NUMBR";
				}
			} else {
				if (float.TryParse (str1, out f1)) {
					if (float.TryParse (str2, out f2)) {
						f1 = float.Parse (str1);
						f2 = float.Parse (str2);
						type = (val1.getType () == "NUMBAR" || val2.getType () == "NUMBAR" ||
						(val1.getType () == "YARN" && val2.getType () == "YARN")) ? "NUMBAR" : "NUMBR";
					}else
						throw new SyntaxException(WarningMessage.cannotConvert(str2));
				} else
					throw new SyntaxException(WarningMessage.cannotConvert(str1));
			}
				

			if (!op.getType ().Equals ("Operator"))
				throw new SyntaxException (WarningMessage.unexpectedLexeme (op.getValue ()));
			else {
				switch (op.getValue()) {
				case Constants.ADD:
					if (type != "TROOF")
						result = Convert.ToString (f1 + f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.ADD, "NUMBR OR NUMBAR"));
					break;
				case Constants.SUB:
					if (type != "TROOF")
						result = Convert.ToString (f1 - f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.SUB, "NUMBR OR NUMBAR"));
					break;
				case Constants.MUL:
					if (type != "TROOF")
						result = Convert.ToString (f1 * f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MUL, "NUMBR OR NUMBAR"));
					break;
				case Constants.DIV:
					if (type != "TROOF")
						result = Convert.ToString (f1 / f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.DIV, "NUMBR OR NUMBAR"));
					break;
				case Constants.MOD:
					if (type != "TROOF")
						result = Convert.ToString (f1 % f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MOD, "NUMBR OR NUMBAR"));
					break;
				case Constants.MAX:
					if (type != "TROOF")
						result = Convert.ToString ((f1 > f2) ? f1 : f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MAX, "NUMBR OR NUMBAR"));
					break;
				case Constants.MIN:
					if (type != "TROOF")
						result = Convert.ToString ((f1 < f2) ? f1 : f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MIN, "NUMBR OR NUMBAR"));
					break;
				case Constants.EQUAL:
					if (type1 == type2 && type1 != "Untyped")
						result = (f1 == f2) ? "WIN" : "FAIL";
					else
						result = "FAIL";
					type = "TROOF";
					break;
				case Constants.NOTEQUAL:
					if (type1 == type2 && type1 != "Untyped")
						result = (f1 != f2) ? "WIN" : "FAIL";
					else
						result = "WIN";
					type = "TROOF";
					break;
				case Constants.AND:
					if (type1 != "TROOF" || type2 != "TROOF")
						throw new SyntaxException (WarningMessage.canAccept(Constants.AND, "TROOF"));
					else result = (b1 && b2) ? "WIN" : "FAIL";
					break;
				case Constants.OR:
					if (type1 != "TROOF" || type2 != "TROOF")
						throw new SyntaxException (WarningMessage.canAccept(Constants.OR, "TROOF"));
					else result = (b1 || b2) ? "WIN" : "FAIL";
					break;
				case Constants.XOR:
					if (type1 != "TROOF" || type2 != "TROOF")
						throw new SyntaxException (WarningMessage.canAccept(Constants.XOR, "TROOF"));
					else result = (b1 ^ b2) ? "WIN" : "FAIL";
					break;
				case Constants.NOT:
					if (type2 != "TROOF")
						throw new SyntaxException (WarningMessage.canAccept(Constants.NOT, "TROOF"));
					else result = (!b2) ? "WIN" : "FAIL";
					break;
				default:
					throw new Exception ("Something went wrong in evaluate");
				}

				if (overPop) {
					stack.Push (temp);
					overPop = false;
				}

				try{
					if (stack.Peek ().getValue () == Constants.NOT) {
						if (type != "TROOF")
							throw new SyntaxException (WarningMessage.unexpectedLexeme (result));
						else
							result = (result == "WIN") ? "FAIL" : "WIN";
					}
				} 
				catch(Exception  e){
					if(e.GetType().Name == "SyntaxException")
						throw e;
				}
				
				stack.Push (new Value(result, type));
			}
		}

		result = stack.Pop ().getValue ();
		if(result != "") Console.WriteLine ("Result in evaluate is: " + result);
		return result;
	}

	private String concatString(List<Lexeme> lexemeList, ref int index){
		String result = "";
		bool hasEnded = false;
		bool needNext = false;
		for (; index < lexemeList.Count; index++) {

			string name = lexemeList [index].getName ();
			string dec = lexemeList [index].getDescription ();

			if (dec.Contains ("comment")) {
				continue;
			} else if (hasEnded)
				throw new SyntaxException (WarningMessage.unexpectedLexeme(name));
			else if (name.Equals(Constants.AN)){
				if (needNext)
					needNext = false;
				else
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.AN));
			} else if (dec.EndsWith ("constant")) {
				result += name;
				needNext = true;
			}
			else if (dec.EndsWith ("Variable Identifier")) {
				if (table.ContainsKey (name)) {
					string varname = name;
					if (table [varname].getValue ().Equals ("NOOB"))
						continue;
					else
						result += table [varname].getValue ();
				} else
					throw new SyntaxException (WarningMessage.varNoDec(name));
				needNext = true;
			} else if (name.Equals (Constants.MKAY)) {
				hasEnded = true;
			} else if(name.Equals("\"")){
				continue;
			} else
				throw new SyntaxException(WarningMessage.unexpectedLexeme(name));
		}
		return result;
	}

	public void UpdateLexemes(List<Lexeme> list){
		foreach (Lexeme l in list) {
			tokensListStore.AppendValues (l.getName(), l.getDescription());
		}
	}

	public void UpdateTables(){
		symbolTableListStore.Clear ();
		foreach (string key in table.Keys) {
			Value val = table [key];
			symbolTableListStore.AppendValues (key, val.getValue (), val.getType ());
		}
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a){
		Gtk.Application.Quit();
		a.RetVal = true;
	}
	
	protected void runProgramClick (object sender, EventArgs e){
		Interpret ();
	}

	private void openFile (object sender, EventArgs e){
		Gtk.FileChooserDialog filechooser =
			new Gtk.FileChooserDialog("Choose the file to open",
				this,
				FileChooserAction.Open,
				"Cancel",ResponseType.Cancel,
				"Open",ResponseType.Accept);

		filechooser.Filter = new FileFilter ();
		filechooser.Filter.Name = "LOLCODE";
		filechooser.Filter.AddPattern("*.lol");
		//"LOLCode Files (.lol)|*.lol|All Files (*.*)|*.*";
		filechooser.SelectMultiple = false;

		if (filechooser.Run() == (int)ResponseType.Accept) 
		{
			sourceText.Buffer.Text = "";

			this.pageLabel.Text = filechooser.Filename;
			System.IO.FileStream file = System.IO.File.OpenRead(filechooser.Filename);
			try{
				using(StreamReader reader = new StreamReader(file)){
					string line = reader.ReadToEnd();
					sourceText.Buffer.Text = line;
					reader.Close();
				}
			}catch(Exception ex){
				this.outputField.Buffer.Text += "\nThe file could not be read: "+ ex.Message;
			}
			file.Close();
		}


		filechooser.Destroy();
	}
	
	protected void CloseOnClick (object sender, EventArgs e)
	{
		Environment.Exit (0);
	}

	protected void runProgramButton (object o, KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.F5) {
			Interpret ();
		}
	}
}