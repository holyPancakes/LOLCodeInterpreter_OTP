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
	private Stack<string> stack = new Stack<string> ();
	private Stack<Value> stackCond = new Stack<Value> ();
	private String line;
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
		stackCond.Clear ();

		table.Add (Constants.IMPLICITVAR, new Value ("NOOB", "Untyped"));

		string sourceString = sourceText.Buffer.Text;
		string[] sourceLines = sourceString.Split (delimeter);
		for(int i=0 ; i< sourceLines.Length; i++) { //infinite loop
			line = sourceLines [i];
			lineField.Buffer.Text = (i+1) + ": " + line;
			try {
				lexemeList = lexer.process (line); //creates an array of lexemes
				UpdateLexemes(lexemeList);

				parse (); //parses the lexemes
				UpdateTables();
			} catch (SyntaxException e) { //if something went wrong, prints the error on screen
				hasError = true;
				outputField.Buffer.Text += "\n" + "ERROR on line " + (i+1) + ": " + e.Message+ "\n";
				break;
			} catch (WarningException e){
				outputField.Buffer.Text += "\n" + "WARNING on line " + (i+1) + ": " + e.Message+ "\n";
			} catch (Exception e){
				outputField.Buffer.Text += "\n" + "CRASHED on line " + (i+1) + ": " + e.Message+ "\n";
				return;
			}
			lexer.reset (); //resets the lexer
		}
		if(!hasEnded && !hasError) outputField.Buffer.Text += "\n" + "ERROR on line " + (sourceLines.Length+1) + ": Program is not closed properly!\n";
		if(hasEnded) lineField.Buffer.Text = "Done!";
	}

	public void parse()
	{ //processses the lexemes
		//Console.WriteLine("\nPARSING:");
		bool nextCommand = false;

		if (lexemeList.Count == 0)
			return;

		for(int i=0; i < lexemeList.Count; i++){
			if (nextCommand)
				throw new SyntaxException (WarningMessage.unexpectedLexeme(lexemeList [i].getName ()));
			else if (lexemeList [i].getName ().Equals (Constants.STARTPROG)) {
				if(hasStarted) 
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.STARTPROG));
				hasStarted = true;
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
					nextCommand = true;
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

					table.Add (Constants.IMPLICITVAR, new Value (value, type));
				} else
					throw new WarningException(WarningMessage.unexpectedLexeme(lexemeList[i].getName()));
			}
			nextCommand = !nextCommand;
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
	}

	private void varDecParse(ref int i){
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
			Lexeme l1 = lexemeList [i + 1]; //gets the next lexeme
			if (l1.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
				if (table.ContainsKey (l1.getName ())) //checks if the variable is in the table
					throw new SyntaxException (WarningMessage.varYesDec (l1.getName ()));
				Lexeme l2 = lexemeList [i + 2]; //gets the next lexeme of next lexeme
				if (l2.getName ().Equals (Constants.STARTINIT)) { //checks if it is ITZ
					Lexeme l3 = lexemeList [i + 3]; //gets the argument of ITZ
					if (l3.getDescription ().EndsWith ("constant")) { //checks if the argument is contstant
						String[] type = l3.getDescription ().Split (delimeter); //gets the datatype of the value
						table.Add (l1.getName (), new Value (l3.getName (), type [0])); //puts it to table
					} else if (l3.getName().Equals("\"")) { //else ITZ has no arguments
						Lexeme l4 = lexemeList [i + 4];
						String[] type = l4.getDescription ().Split (delimeter); //gets the datatype of the value
						table.Add (l1.getName (), new Value (l4.getName (), type [0])); //puts it to table
						i+=2;
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
			i += 3;
		} else
			throw new SyntaxException (WarningMessage.noArguments (Constants.VARDEC));
	}

	private void assignParse(ref int i){
		Lexeme var = lexemeList [i - 1]; //gets the left and right lexemes of R
		Lexeme value = lexemeList [i + 1];
		if (var.getDescription ().Equals ("Variable Identifier")) { //checks if the left side is a variable
			if (value.getDescription ().EndsWith ("constant")) { //checks if the right side is a constant
				String[] type = value.getDescription ().Split (delimeter); //gets the dataype
				Value old = table [var.getName ()]; //gets the old datatype of the variable

				table [var.getName ()] = new Value (value.getName (), type [0]); //puts new value to table
			} else if(value.getName().Equals("\"")){
				Lexeme yarn = lexemeList [i + 2];
				String[] type = yarn.getDescription ().Split (delimeter); //gets the dataype
				Value old = table [var.getName ()]; //gets the old datatype of the variable

				table [var.getName ()] = new Value (yarn.getName (), type [0]); //puts new value to table
				i+=2;
			}else if (value.getDescription ().Equals ("Variable Identifier")) { //checks if the right side is a variable
				if (!table.ContainsKey (value.getName ())) //check if the variable is already declared
					throw new SyntaxException (WarningMessage.varNoDec (value.getName ()));

				table.Add (var.getName (), table [value.getName ()]); //changes the value of the variable
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

		string name = lexemeList [index].getName ();;
		string result = "";

		if (name.Equals (Constants.CONCAT)) {
			index++;
			result = concatString (lexemeList, ref index);
		} else if (name.Equals (Constants.EQUAL) || name.Equals(Constants.NOTEQUAL)){
			result = compareParse(lexemeList, ref index);
		}
		else
			throw new WarningException(WarningMessage.unexpectedLexeme(name));
		
		return result;
	}

	private String compareParse(List<Lexeme> lexemeList, ref int index){
		string result = "";
		string op = "";
		string name = "";

		int glPointer = 0;
		int i = 0;

		bool equals = false;
		bool isGreater = false;
		bool isLess = false;
		bool needAN = false;

		char[] delimiter = {' '};

		foreach(Lexeme l in lexemeList){
			name = l.getName();
			string dec = l.getDescription ();

			if (name.Equals (Constants.EQUAL))
				equals = true;
			else if (name.Equals (Constants.MAX)) {
				isGreater = true;
				glPointer = i;
			} else if (name.Equals (Constants.MIN)) {
				isLess = true;
				glPointer = i;
			} else if (name.Equals (Constants.AN) || name.Equals ("\"") || dec.Equals ("Variable Identifier") || dec.EndsWith ("constant"))
				continue;
			else
				throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			i++;
		}

		if(equals && isGreater && isLess)
			throw new Exception(WarningMessage.unexpectedLexeme(Constants.MAX + " and " + Constants.MIN));
		else if(equals && isGreater)
			op = ">=";
		else if(equals && isLess)
			op = "<=";
		else if(equals)
			op = "=";
		else if(isGreater)
			op = ">";
		else if(isLess)
			op = "<";
		else
			op = "!=";

		stackCond.Push(new Value(op, "Operator"));
		Console.WriteLine (op);

		if(op.Equals(">=") || op.Equals("<=") || op.Equals(">") || op.Equals("<")){
			index++;
			if(lexemeList[index].getName().Equals("\"")) index++;

			glPointer++;
			if(lexemeList[glPointer].getName().Equals("\"")) glPointer++;

			name = lexemeList [index].getName ();
			Value startValue;

			if(!name.Equals(lexemeList[glPointer].getName()))
				throw new SyntaxException(WarningMessage.shouldSame(name, lexemeList[glPointer].getName()));
			else if(lexemeList[index].getDescription().Contains("constant")){
				string[] type = lexemeList[index].getDescription().Split(delimiter);
				startValue = new Value(lexemeList[index].getName(), type[0]);
			} else if(lexemeList[index].getDescription().Equals("Variable Identifier")){
				if(!table.ContainsKey(name))
					throw new SyntaxException(WarningMessage.varNoDec(name));
				startValue = table[name];
			} else
				throw new SyntaxException(WarningMessage.unexpectedLexeme(name));

			index = glPointer;
		}

		for(; index < lexemeList.Count; index++){
			name = lexemeList [index].getName ();
			string dec = lexemeList [index].getDescription ();

			if(name.Equals("\""))
				continue;
			else if(dec.Contains("constant")){
				if(needAN)
					throw new SyntaxException(WarningMessage.unexpectedLexeme(name));
				string[] type = dec.Split(delimiter);
				Value val = new Value(name, type[0]);
				stackCond.Push(val);
				needAN = true;
			} else if(dec.Equals("Variable Identifier")){
				if(needAN)
					throw new SyntaxException(WarningMessage.unexpectedLexeme(name));
				else if(!table.ContainsKey(name))
					throw new SyntaxException(WarningMessage.varNoDec(name));

				Value val = table[name];
				stackCond.Push(val);

				needAN = true;
			} else if(name.Equals(Constants.AN)){
				needAN = false;
			} else
				throw new SyntaxException(WarningMessage.unexpectedLexeme(name));
		}

		result = evaluateCond ();
		
		return result;
	}

	private String evaluateCond (){		
		Value val1 = null;
		Value val2 = null;
		Value op = null;

		try{
			val2 = stackCond.Pop();
			val1 = stackCond.Pop();
			op = stackCond.Pop();
		} catch(Exception){
			throw new SyntaxException (WarningMessage.lackOperands());
		}

		if (!op.getType ().Equals ("Operator"))
			throw new SyntaxException (WarningMessage.unexpectedLexeme (op.getValue()));

		switch (op.getValue()) {
		case "=":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue());
					float f2 = float.Parse (val2.getValue());

					if (f1 == f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (val1.getValue().Equals (val2.getValue()))
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "FAIL";
		case "!=":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue());
					float f2 = float.Parse (val2.getValue());

					if (f1 != f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (!val1.getValue().Equals (val2.getValue()))
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "WIN";
		case ">=":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue());
					float f2 = float.Parse (val2.getValue());

					if (f1 >= f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (val1.getValue().CompareTo (val2.getValue()) >= 0)
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "FAIL";
		case "<=":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue ());
					float f2 = float.Parse (val2.getValue ());

					if (f1 <= f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (val1.getValue ().CompareTo (val2.getValue ()) <= 0)
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "FAIL";
		case ">":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue());
					float f2 = float.Parse (val2.getValue());

					if (f1 > f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (val1.getValue().CompareTo (val2.getValue()) > 0)
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "WIN";
		case "<":
			if (val1.getType ().Equals (val2.getType ()) || val1.getType ().Equals ("NOOB")) {
				if (Constants.NUMBARVAL.Match (val1.getValue()).Success) {
					float f1 = float.Parse (val1.getValue());
					float f2 = float.Parse (val2.getValue());

					if (f1 != f2)
						return "WIN";
					else
						return "FAIL";
				} else {
					if (val1.getValue().CompareTo (val2.getValue()) < 0)
						return "WIN";
					else
						return "FAIL";	
				}
			} else
				return "WIN";
		default:
			throw new Exception ("Something went wrong in evaluate...");
		}
	}

	private String concatString(List<Lexeme> lexemeList, ref int index){
		String result = "";
		bool hasEnded = false;
		bool needNext = false;
		for (; index < lexemeList.Count; index++) {

			string name = lexemeList [index].getName ();
			string dec = lexemeList [index].getDescription ();

			if (hasEnded)
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
		if (table.Count != 0) {
			foreach (string key in table.Keys) {
				Value val = table [key];
				symbolTableListStore.Clear ();
				symbolTableListStore.AppendValues (key, val.getValue (), val.getType ());
			}
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
		filechooser.Filter.Name = "LolCode";
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

	protected void foo (object o, KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.F5) {
			Interpret ();
		}
	}

	protected void runProgramButton (object o, KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.F5) {
			Interpret ();
		}
	}
}