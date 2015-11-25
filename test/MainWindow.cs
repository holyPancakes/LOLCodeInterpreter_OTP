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
		char[] delimeter = {'\n'};

		Console.WriteLine (sourceText.Buffer.Text);
		Console.WriteLine ("==========");
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

		table.Add (Constants.IMPLICITVAR, new Value (Constants.NULL, Constants.NOTYPE));

		string sourceString = sourceText.Buffer.Text;
		string[] sourceLines = sourceString.Split (delimeter);
		int i = 0;
		int junk = 0;
		try {
			lineField.Buffer.Text = "Creating Lexemes...";
			lexemeList = lexer.process (sourceLines, ref i); //creates an array of lexemes
			i=0;
			UpdateLexemes(lexemeList);
			lineField.Buffer.Text = "Running the Lexemes...";
			parse (ref i,ref junk, Constants.STARTPROG); //parses the lexemes
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
	}

	public void parse(ref int lineNo, ref int i, string fromWhere){
		bool nextCommand = false;
		char[] space = { ' ' };
		string desc;
		string name;

		if (lexemeList.Count == 0)
			return;

		for(; i < lexemeList.Count; i++){
			desc = lexemeList [i].getDescription ();
			name = lexemeList [i].getName ();

			if (desc.Contains ("comment")) {
				if (name == Constants.ENDCOMMENT || name == Constants.ONELINE) {
					nextCommand = true;
				}
			} else if (desc.Contains ("break")) {
				nextCommand = false;
				if(name == Constants.EOL) lineNo++;
			} else if (nextCommand) {
				throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i].getName ()));
			}else if (name == Constants.STARTPROG) {
				if(hasStarted) 
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.STARTPROG));
				hasStarted = true;
			} else if (!hasStarted)
				throw new SyntaxException (WarningMessage.noStart());
			else if(hasEnded)
				throw new SyntaxException(WarningMessage.alreadyEnd());
			else{
				if (name == Constants.PRINT) { //checks if the keyword is VISIBLE
					printParse (ref i);
					nextCommand = true;
				} else if (name == Constants.SCAN) {
					scanParse (ref i);
					nextCommand = true;
				} else if (name == Constants.VARDEC) { //checks if the keyword is I HAS A
					varDecParse (ref i);
					nextCommand = true;
				} else if (name == Constants.ASSIGN) { //checks if the keyword is R
					assignParse (ref i);
					nextCommand = true;
				} else if (desc == Constants.VARDESC) { //checks if the keyword is KTHXBYE
					if (!table.ContainsKey (lexemeList [i].getName ())) //check if the variable is already declared
						throw new SyntaxException (WarningMessage.varNoDec (lexemeList [i].getName ()));
					else if (lexemeList [i + 1].getName () == Constants.ASSIGN)
						continue;
					else { 
						table [Constants.IMPLICITVAR] = table [lexemeList [i].getName ()];
						nextCommand = true;
					}
				} else if (name == Constants.ENDPROG) { //checks if the keyword is KTHXBYE
					hasEnded = true;
					nextCommand = true;
				} else if (desc.Contains ("Operator")) {
					string value = operatorList (lexemeList, ref i);
					string type = "";
					if (Constants.INTVAL.Match (value).Success)
						type = Constants.INT;
					else if (Constants.FLOATVAL.Match (value).Success)
						type = Constants.FLOAT;
					else if (Constants.BOOLVAL.Match (value).Success)
						type = Constants.BOOL;
					else
						type = Constants.STRING;
					table [Constants.IMPLICITVAR] = new Value (value, type);
					nextCommand = true;
				} else if (desc.Contains ("constant")) {
					if (lexemeList [i - 1].getName () != Constants.CASE) {
						string[] type = desc.Split (space);
						table [Constants.IMPLICITVAR] = new Value (lexemeList [i].getName (), type [0]);
					}
					nextCommand = true;
				} else if (name == Constants.SWITCH) {
					switchParse (ref i);
					nextCommand = true;
					i--;
				} else if (name == Constants.CONDITION) {
					ifParse (ref i);
					nextCommand = true;
					i--;
				} else if (name == Constants.END_IF || 
					name == Constants.BREAK || 
					name == Constants.ELSE) {
					if (fromWhere == Constants.SWITCH &&
						(name == Constants.END_IF || name == Constants.BREAK)) {
						return;
					} else if (fromWhere == Constants.IF && 
						(name == Constants.END_IF || name == Constants.ELSE)) {
						return;
					} else 
						throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i].getName ()));
				} else if (name == Constants.CASE || name == Constants.DEFAULT) {
					if(fromWhere == Constants.STARTPROG) 
						throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i].getName ()));
				}else
					throw new WarningException(WarningMessage.unexpectedLexeme(lexemeList[i].getName()));
			}
			UpdateTables();
		}
	}

	private void ifParse(ref int i){
		int elseindex = 0;
		int ifIndex = 0;
		int index = 0;
		string value = table [Constants.IMPLICITVAR].getValue ();
		string name = lexemeList [i].getName ();
		string desc = lexemeList [i].getDescription ();

		for (;  name != Constants.END_IF; i++) {
			desc = lexemeList [i].getDescription ();
			name = lexemeList [i].getName ();
			if (!desc.Contains ("constant")) {
				if (name == Constants.IF) {
					ifIndex = i + 1;
				} else if (name == Constants.ELSE) {
					elseindex = i + 1;
				} else if (name == Constants.ENDPROG) {
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDPROG));
				}
			}
		}
		if (ifIndex == 0)
			throw new SyntaxException (WarningMessage.noIF ());
		
		index = (value == Constants.TRUE) ? ifIndex : elseindex;

		if (index == 0)
			return;
		
		int junk = 0;
		parse (ref junk, ref index, Constants.IF);
	}

	private void switchParse(ref int i){
		Dictionary<string,int> cases = new Dictionary<string,int> ();
		int index = 0;
		string name = lexemeList [i].getName ();
		string desc = lexemeList [i].getDescription ();
		string value = table [Constants.IMPLICITVAR].getValue ();

		for (; name != Constants.END_IF; i++) {
			name = lexemeList [i].getName ();
			desc = lexemeList [i].getDescription ();
			if (!desc.Contains ("constant")) {
				if (name == Constants.CASE) {
					if (lexemeList [i + 1].getName ().Equals ("\"")) {
						cases.Add (lexemeList [i + 2].getName (), i + 4);
					} else if (lexemeList [i + 1].getDescription ().Contains ("constant")) {
						cases.Add (lexemeList [i + 1].getName (), i + 2);
					} else {
						throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i + 1].getName ()));
					}
				} else if (name == Constants.ENDPROG){
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDPROG));
				} else if (name == Constants.DEFAULT) {
					cases.Add (Constants.DEFAULT, i + 1);
				}
			}
		}

		foreach (string  g in cases.Keys) {
			if (value == g) {
				index = cases [g];
			}
		}

		if (index == 0) {
			if (cases.ContainsKey (Constants.DEFAULT))
				index = cases [Constants.DEFAULT];
			else
				return;
		}

		int junk = 0;
		parse (ref junk, ref index, Constants.SWITCH);
	}

	private void printParse(ref int i){
		string name = lexemeList [i+1].getName ();
		string desc = lexemeList [i+1].getDescription ();
		string toWrite;

		if (desc.Contains("constant") || desc.Contains("Operator") ||
			desc == Constants.VARDESC || name == "\"") {
			if (name == "\"") {
				Lexeme yarn = lexemeList [i + 2];
				toWrite = yarn.getName () + "\n";
				i += 3;
			} else if (desc.Contains("constant")) {
				toWrite = name + "\n";
				i++;
			} else if (desc == Constants.VARDESC) {
				if (!table.ContainsKey (name))
					throw new SyntaxException (WarningMessage.varNoDec (name));
				Value val = table [name];
				toWrite = val.getValue () + "\n";
				i++;
			} else if (desc.Contains ("Operator")) {
				i++;
				toWrite = operatorList (lexemeList, ref i);
				i--;
			} else
				throw new SyntaxException (WarningMessage.notPrintable (name));
			outputField.Buffer.Text += toWrite;
		} else
			throw new SyntaxException (WarningMessage.noArguments (Constants.PRINT));
	}

	private void scanParse(ref int i){
		string name = lexemeList [i+1].getName ();
		string desc = lexemeList [i+1].getDescription ();

		if (desc == Constants.VARDESC) {
			if (table.ContainsKey (name)) {
				Dialog inputPrompt = null;
				ResponseType response = ResponseType.None;

				try {
					inputPrompt = new Dialog (Constants.SCAN, this, 
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
							Value val = new Value (input, Constants.STRING);
							table [lexemeList [i + 1].getName ()] = val; 
						}
						inputPrompt.Destroy ();
					}
				}
			} else
				throw new SyntaxException (WarningMessage.varNoDec (name));
		} else
			throw new SyntaxException (WarningMessage.noArguments (Constants.SCAN));
		i++;
	}

	private void varDecParse(ref int i){
		string name = lexemeList [i+1].getName ();
		string desc = lexemeList [i+1].getDescription ();
		char[] spaces = { ' ' };

		if (lexemeList[i+2].getName() != Constants.STARTINIT) {
			if (desc == Constants.VARDESC) {
				if (table.ContainsKey (name))
					throw new SyntaxException (WarningMessage.varYesDec (name));
				table.Add (name, new Value (Constants.NULL, Constants.NOTYPE));
			} else
				throw new SyntaxException (WarningMessage.expectedWord (Constants.VARDESC, Constants.VARDEC));
			i++;
		} else {
			if (desc == Constants.VARDESC) {
				if (table.ContainsKey (name))
					throw new SyntaxException (WarningMessage.varYesDec (name));
				Lexeme l2 = lexemeList [i + 2];
				if (l2.getName ().Equals (Constants.STARTINIT)) {
					Lexeme l3 = lexemeList [i + 3];
					if (l3.getDescription ().EndsWith ("constant")) {
						String[] type = l3.getDescription ().Split (spaces);
						table.Add (name, new Value (l3.getName (), type [0]));
					} else if (l3.getName ().Equals ("\"")) {
						Lexeme l4 = lexemeList [i + 4];
						String[] type = l4.getDescription ().Split (spaces);
						table.Add (name, new Value (l4.getName (), type [0]));
						i += 2;
					} else if (table.ContainsKey (l3.getName ())) {
						table.Add (name, table [l3.getName ()]);
					} else if (l3.getDescription ().Contains ("Operator")) {
						i += 3;
						string val = operatorList (lexemeList, ref i);
						string type = returnType(val);

						table.Add (name, new Value (val, type));
					} else
						throw new SyntaxException (WarningMessage.expectedWord ("constant or variable", Constants.STARTINIT));
				} else
					throw new SyntaxException (WarningMessage.expectedWord (Constants.STARTINIT, "variable declaration"));
			} else
				throw new SyntaxException (WarningMessage.expectedWord ("variable declaration", Constants.STARTINIT));
			i += 3;
		}
	}

	private void assignParse(ref int i){
		string varName = lexemeList [i-1].getName ();
		string varDesc = lexemeList [i-1].getDescription ();
		string valueName = lexemeList [i+1].getName ();
		string valueDesc = lexemeList [i+1].getDescription ();
		char[] spaces = { ' ' };

		if (varDesc == Constants.VARDESC) {
			if (valueDesc.EndsWith ("constant")) {
				String[] type = valueDesc.Split (spaces);
				Value old = table [varName];

				table [varName] = new Value (valueName, type [0]);
			} else if(valueName == "\""){
				Lexeme yarn = lexemeList [i + 2];
				String[] type = yarn.getDescription ().Split (spaces);
				Value old = table [varName];

				table [varName] = new Value (yarn.getName (), type [0]);
				i+=2;
			}else if (valueDesc == Constants.VARDESC) {
				if (!table.ContainsKey (valueName))
					throw new SyntaxException (WarningMessage.varNoDec (valueName));

				table[varName] = table [valueName];
			} else if (valueDesc.Contains ("Operator")) {
				i++;
				string val = operatorList (lexemeList, ref i);
				string type = returnType (val);

				table [varName] = new Value (val, type);
			} else
				throw new SyntaxException (WarningMessage.RRightSide ());

		} else
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

		index--;
		return result;
	}

	private String mathOperation(List<Lexeme> lexemeList, ref int index){
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";
		for (; !lexemeList [index].getDescription ().Contains ("break"); index++) {
			String name = lexemeList [index].getName ();
			String desc = lexemeList [index].getDescription ();

			if (desc.Contains ("comment")) {
				continue;
			} else if (name == Constants.AN) {
				if (ANned)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else
					ANned = true;
			} else if (Constants.INTVAL.Match (name).Success ||
			           Constants.FLOATVAL.Match (name).Success ||
			           Constants.BOOLVAL.Match (name).Success) {
				if (ANned) {
					String[] type = desc.Split (delimiter);
					stack.Push (new Value (name, type [0]));
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			} else if (desc == Constants.VARDESC) {
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
		return result;
	}

	private String arityOperation(List<Lexeme> lexemeList, ref int index){
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";
		for (; !lexemeList [index].getDescription ().Contains ("break"); index++) {
			String name = lexemeList [index].getName ();
			String desc = lexemeList [index].getDescription ();

			if (desc.Contains ("comment")) {
				continue;
			} else if (name == Constants.AN) {
				if (ANned)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else
					ANned = true;
			} else if (Constants.INTVAL.Match (name).Success ||
				Constants.FLOATVAL.Match (name).Success ||
				Constants.BOOLVAL.Match (name).Success) {
				if (ANned) {
					String[] type = desc.Split (delimiter);
					stackArity.Push (new Value (name, type [0]));
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			} else if (desc == Constants.VARDESC){
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
			popped = stackArity.Pop();

			if (popped.getValue () == Constants.MKAY) {
				if (!start)
					throw new SyntaxException (WarningMessage.unexpectedLexeme (popped.getValue ()));
				else {
					start = false;
					continue;
				}
			} else if (stackArity.Peek ().getValue () == Constants.NOT) {
				stackArity.Pop ();
				values.Add(new Value((popped.getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.FALSE));
			} else if (popped.getType () != Constants.BOOL) {
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
					result = (result == Constants.TRUE) ? Constants.FALSE : Constants.TRUE;
				}
				values.Add (new Value (result, Constants.BOOL));
			} else {
				values.Add (popped);
			}

			start = false;
		}
		
		popped = stackArity.Pop ();
		if (popped.getValue () == Constants.MANY_AND) {
			foreach (Value v in values) {
				if (v.getValue () == Constants.FALSE) {
					result = Constants.FALSE;
					break;
				}
			}
			if (result != Constants.FALSE)
				result = Constants.TRUE;
		} else if (popped.getValue () == Constants.MANY_OR) {
			foreach (Value v in values) {
				if (v.getValue () == Constants.TRUE) {
					result = Constants.TRUE;
				}
			}
			if (result != Constants.TRUE)
				result = Constants.FALSE;
		} else
			throw new SyntaxException(WarningMessage.unexpectedLexeme(popped.getValue()));
		return result;
	}

	private String evaluateCond (){
		Value val1 = new Value(Constants.NULL, Constants.NOTYPE);
		Value val2 = new Value(Constants.NULL, Constants.NOTYPE);
		Value op = null;
		Value temp = null;
		String result = "";
		bool overPop = false;

		while(stack.Count > 1){

			try{
				val2 = stack.Pop();
				if(stack.Count > 1){
					if(stack.Peek().getValue() == Constants.NOT){
						if(val2.getType() != Constants.BOOL)
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val2.getValue()));
						else{
							stack.Pop();
							val2 = new Value((val2.getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.BOOL);
						}
					}
				}
				if(stack.Count > 1){
					val1 = stack.Pop();
					if(stack.Peek().getValue() == Constants.NOT){
						if(val2.getType() != Constants.BOOL)
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val2.getValue()));
						else{
							stack.Pop();
							val2 = new Value((val2.getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.BOOL);
						}
					}
				}
				op = stack.Pop();
				if(op.getType() != "Operator"){
					overPop = true;

					temp = val2;
					val2 = val1;
					val1 = op;
					op = stack.Pop();
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

			if (type1 == Constants.BOOL || type2 == Constants.BOOL) {
				if (type1 == Constants.BOOL) {
					b1 = (str1 == Constants.TRUE) ? true : false;
					if (type2 == Constants.BOOL)
						b2 = (str2 == Constants.TRUE) ? true : false;
					else
						f2 = float.Parse (str2);
					type = Constants.BOOL;
				} else if (type2 == Constants.BOOL) {
					b2 = (str2 == Constants.TRUE) ? true : false;
					if(type1 != Constants.NOTYPE) f1 = float.Parse (str1);
					type = Constants.BOOL;
				}else {
					f1 = float.Parse (str1);
					f2 = float.Parse (str2);
					type = (val1.getType () == Constants.FLOAT || val2.getType () == Constants.FLOAT) ? Constants.FLOAT : Constants.INT;
				}
			} else {
				if (float.TryParse (str1, out f1)) {
					if (float.TryParse (str2, out f2)) {
						f1 = float.Parse (str1);
						f2 = float.Parse (str2);
						type = (val1.getType () == Constants.FLOAT || val2.getType () == Constants.FLOAT ||
							(val1.getType () == Constants.STRING && val2.getType () == Constants.STRING)) ? Constants.FLOAT : Constants.INT;
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
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 + f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.ADD, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.SUB:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 - f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.SUB, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MUL:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 * f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MUL, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.DIV:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 / f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.DIV, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MOD:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 % f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MOD, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MAX:
					if (type != Constants.BOOL)
						result = Convert.ToString ((f1 > f2) ? f1 : f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MAX, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MIN:
					if (type != Constants.BOOL)
						result = Convert.ToString ((f1 < f2) ? f1 : f2);
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MIN, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.EQUAL:
					if (type1 == type2 && type1 != Constants.NOTYPE)
						result = (f1 == f2) ? Constants.TRUE : Constants.FALSE;
					else
						result = Constants.FALSE;
					type = Constants.BOOL;
					break;
				case Constants.NOTEQUAL:
					if (type1 == type2 && type1 != Constants.NOTYPE)
						result = (f1 != f2) ? Constants.TRUE : Constants.FALSE;
					else
						result = Constants.TRUE;
					type = Constants.BOOL;
					break;
				case Constants.AND:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.AND, Constants.BOOL));
					else result = (b1 && b2) ? Constants.TRUE : Constants.FALSE;
					break;
				case Constants.OR:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.OR, Constants.BOOL));
					else result = (b1 || b2) ? Constants.TRUE : Constants.FALSE;
					break;
				case Constants.XOR:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.XOR, Constants.BOOL));
					else result = (b1 ^ b2) ? Constants.TRUE : Constants.FALSE;
					break;
				case Constants.NOT:
					if (type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.NOT, Constants.BOOL));
					else result = (!b2) ? Constants.TRUE : Constants.FALSE;
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
						if (type != Constants.BOOL)
							throw new SyntaxException (WarningMessage.unexpectedLexeme (result));
						else
							result = (result != Constants.TRUE) ? Constants.TRUE : Constants.FALSE;
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
			else if (dec.EndsWith (Constants.VARDESC)) {
				if (table.ContainsKey (name)) {
					string varname = name;
					if (table [varname].getValue ().Equals (Constants.NULL))
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

	public String returnType(String val){
		String type = "";

		if (Constants.INTVAL.Match (val).Success)
			type = Constants.INT;
		else if (Constants.FLOATVAL.Match (val).Success)
			type = Constants.FLOAT;
		else if (Constants.BOOLVAL.Match (val).Success)
			type = Constants.BOOL;
		else
			type = Constants.STRING;

		return type;
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