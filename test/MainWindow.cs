using System;
using Gtk;
using test;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;


public partial class MainWindow: Gtk.Window
{	
	LexemeCreator lexer = new LexemeCreator(); //for lexical analysis
	List<Lexeme> lexemeList = new List<Lexeme>();
	Stack<string> stack = new Stack<string> ();
	String line;
	char[] delimeter = {'\n'};

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
	}

	public void Interpret(){
		Console.WriteLine ("Interpret!");
		outputField.Buffer.Text = "";
		hasStarted = false;
		hasEnded = false;
		hasError = false;
		tokensListStore.Clear ();
		symbolTableListStore.Clear ();
		lexemeList.Clear ();
		table.Clear ();
		stack.Clear ();

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
		char[] delimeter = {' '};
		bool nextCommand = false;

		if (lexemeList.Count == 0)
			return;
		
		for(int i=0; i < lexemeList.Count; i++){
			if (nextCommand)
				throw new SyntaxException ("Expression already ended. Put " + lexemeList [i].getName () + " on next line.");
			else if (lexemeList [i].getName ().Equals (Constants.STARTPROG)) {
				if(hasStarted) throw new SyntaxException ("Unexpected " + Constants.STARTPROG + "!");
				hasStarted = true;
			} else if (!hasStarted)
				throw new SyntaxException ("Program has not started yet!");
			else if(hasEnded)
				throw new SyntaxException("Program already ended!");
			else{
				if (lexemeList [i].getName ().Equals (Constants.PRINT)) { //checks if the keyword is VISIBLE
					if (lexemeList.Count - i >= 2) { //checks if VISIBLE has arguments
						Lexeme l = lexemeList [i + 1]; //gets the next lexeme
						String toWrite;
						if (l.getName ().Equals ("\"")) { //checks if it is a string constant
							Lexeme yarn = lexemeList [i + 2];
							toWrite = yarn.getName () + "\n"; //prints the string
							i += 3;
						} else if (l.getDescription ().Equals ("Variable Identifier")) {
							if (!table.ContainsKey (l.getName ())) //checks if the variable is already declared
								throw new SyntaxException ("Variable identifier '" + l.getName () + "' is not yet declared.");
							Value val = table [l.getName ()]; //gets the value of the variable
							toWrite = val.getValue () + "\n"; //prints the value of the variable
							i++;
						} else if (l.getDescription ().Contains ("Operator")) {
							i++;
							toWrite = operatorList (lexemeList, ref i);
						} else
							throw new SyntaxException (l.getName () + " is not printable!"); //error that a value is not printable
						outputField.Buffer.Text += toWrite;
					} else //else VISIBLE has no arguments
						throw new SyntaxException ("VISIBLE has no arguments!");
				} else if (lexemeList [i].getName ().Equals (Constants.SCAN)) {
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
							throw new SyntaxException ("Variable " + lexemeList [i + 1].getName () + " is not yet delcared!");
					} else
						throw new SyntaxException ("GIMMEH has no arguments!");
				} else if (lexemeList [i].getName ().Equals (Constants.VARDEC)) { //checks if the keyword is I HAS A
					if (lexemeList.Count - i == 2) { //checks if I HAS A has arguments
						Lexeme l = lexemeList [i + 1]; //gets the next lexeme
						if (l.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
							if (table.ContainsKey (l.getName ())) //checks if the variable is already in the table
								throw new SyntaxException ("Variable identifier " + l.getName () + " already exists.");
							table.Add (l.getName (), new Value ("NOOB", "Untyped")); //makes the variable NOOB
						} else
							throw new SyntaxException ("Expected variable declaration after " + Constants.VARDEC);
						i++; //increments the index
					} else if (lexemeList.Count > 2) { //checks if the statement also starts with a value
						Lexeme l1 = lexemeList [i + 1]; //gets the next lexeme
						if (l1.getDescription ().Equals ("Variable Identifier")) { //checks if it is a variable identifier
							if (table.ContainsKey (l1.getName ())) //checks if the variable is in the table
								throw new SyntaxException ("Variable " + l1.getName () + " already exists.");
							Lexeme l2 = lexemeList [i + 2]; //gets the next lexeme of next lexeme
							if (l2.getName ().Equals (Constants.STARTINIT)) { //checks if it is ITZ
								Lexeme l3 = lexemeList [i + 3]; //gets the argument of ITZ
								if (l3.getDescription ().EndsWith ("constant")) { //checks if the argument is contstant
									String[] type = l3.getDescription ().Split (delimeter); //gets the datatype of the value
									table.Add (l1.getName (), new Value (l3.getName (), type [0])); //puts it to table
								} else if (table.ContainsKey (l3.getName ())) { //checks if the argument is a variable and it is initialized
									table.Add (l1.getName (), table [l3.getName ()]); //copies the value and puts it to the table
								} else if (l3.getDescription ().Contains ("Operator")) {
									i += 3;
									string val = operatorList (lexemeList, ref i);
									string type = "";
									if (Constants.NUMBRVAL.Match (val).Success)
										type = "NUMBR";
									else if (Constants.NUMBARVAL.Match(val).Success)
										type = "NUMBAR";
									else if (Constants.TROOFVAL.Match(val).Success)
										type = "TROOF";
									else 
										type = "YARN";

									table.Add (l1.getName (), new Value (val, type));
								} else //else ITZ has no arguments
									throw new SyntaxException ("Expected constant or variable after ITZ.");
							} else
								throw new SyntaxException ("Expected " + Constants.STARTINIT + " after variable declaration!");
						} else
							throw new SyntaxException ("Expected variable declaration after " + Constants.VARDEC);
						i += 3;
					} else
						throw new SyntaxException ("I HAS A has no arguments!");
				} else if (lexemeList [i].getName ().Equals (Constants.ASSIGN)) { //checks if the keyword is R
					Lexeme var = lexemeList [i - 1]; //gets the left and right lexemes of R
					Lexeme value = lexemeList [i + 1];
					if (var.getDescription ().Equals ("Variable Identifier")) { //checks if the left side is a variable
						if (value.getDescription ().EndsWith ("constant")) { //checks if the right side is a constant
							String[] type = value.getDescription ().Split (delimeter); //gets the dataype
							Value old = table [var.getName ()]; //gets the old datatype of the variable

							table [var.getName ()] = new Value (value.getName (), type [0]); //puts new value to table
						} else if (value.getDescription ().Equals ("Variable Identifier")) { //checks if the right side is a variable
							if (!table.ContainsKey (value.getName ())) //check if the variable is already declared
								throw new SyntaxException ("Variable " + value.getName () + " is not yet declared!");
		
							table.Add (var.getName (), table [value.getName ()]); //changes the value of the variable
						} else if (value.getDescription ().Contains ("Operator")) {
							i++;
							string val = operatorList (lexemeList, ref i);
							string type = "";
							if (Constants.NUMBRVAL.Match (val).Success)
								type = "NUMBR";
							else if (Constants.NUMBARVAL.Match(val).Success)
								type = "NUMBAR";
							else if (Constants.TROOFVAL.Match(val).Success)
								type = "TROOF";
							else 
								type = "YARN";

							table[var.getName ()] = new Value (val, type);
						} else //else the right side is neither a variable or a constant
							throw new SyntaxException ("Only variable or constants should be on left hand side of R");

					} else //else the left side is not a variable
						throw new SyntaxException ("Variable should be on left hand side of R. " + var.getName () + " is not a variable.");

					i++;
				} else if (lexemeList [i].getDescription ().Equals ("Variable Identifier")) { //checks if the keyword is KTHXBYE
					if (!table.ContainsKey (lexemeList [i].getName ())) //check if the variable is already declared
						throw new SyntaxException ("Variable " + lexemeList [i].getName () + " is not yet declared!");
					nextCommand = true;
				} else if (lexemeList [i].getName ().Equals (Constants.ENDPROG)) { //checks if the keyword is KTHXBYE
					hasEnded = true;
				} else if (lexemeList[i].getDescription ().Contains ("Operator")) {
					operatorList (lexemeList, ref i);
				} else
					throw new WarningException("Unexpected lexeme " + lexemeList[i].getName() + " found!");
			}
			nextCommand = !nextCommand;
		}
		//Console.WriteLine("========");
	}

	private String operatorList(List<Lexeme> lexemeList, ref int index){
		string name = lexemeList [index].getName ();
		string result = "";
		bool needAN = false;

		if (index + 1 == lexemeList.Count)
			throw new SyntaxException (name + " has no arguments!");

		index++;
		if (name.Equals (Constants.CONCAT))
			result = concatString (lexemeList, ref index);
		else if (name.Equals (Constants.EQUAL)){
			stack.Push ("=");
			if (lexemeList.Count > 4)
				throw new SyntaxException (Constants.EQUAL + " has so many operands!");
			
			for (; index < lexemeList.Count; index++, needAN = !needAN) {
				name = lexemeList [index].getName ();
				string dec = lexemeList[index].getDescription();

				if (name.Equals (Constants.AN))
					continue;
				else if (needAN)
					throw new SyntaxException ("Expected " + Constants.AN);
				else if (dec.EndsWith ("constant"))
					stack.Push (name);
				else if (dec.Equals ("Variable Identifier")) {
					if (!table.ContainsKey (name))
						throw new SyntaxException (name + "is not yet declared!");
					Value val = table [name];
					stack.Push (val.getValue ());
				} else
					throw new WarningException ("None of the above in EQUAL");
			}

			if (!needAN)
				throw new SyntaxException ("Lack of operands");
			else
				result = evaluate ();
			Console.WriteLine (result);
		}
		else
			throw new WarningException("Unexpected operator " + name + " found!");
		
		return result;
	}

	private String concatString(List<Lexeme> lexemeList, ref int index){
		String result = "";
		bool hasEnded = false;
		bool needNext = false;
		for (; index < lexemeList.Count; index++) {
			string name = lexemeList [index].getName ();
			string dec = lexemeList [index].getDescription ();

			if (hasEnded)
				throw new SyntaxException ("Unexpected " + name + " found!");
			else if (name.Equals(Constants.AN)){
				if (needNext)
					needNext = false;
				else
					throw new SyntaxException ("Needs " + Constants.AN + " to separate the arguments.");
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
					throw new SyntaxException ("Variable " + name+ " is not declared!");
				needNext = true;
			} else if (name.Equals (Constants.MKAY)) {
				hasEnded = true;
			} else if(name.Equals("\"")){
				continue;
			} else
				throw new SyntaxException( "Unexpected " + name + " found!");
		}
		return result;
	}

	private String evaluate(){		
		String val1 = "";
		String val2 = "";
		String op = "";
		try{
			val1 = stack.Pop();
			val2 = stack.Pop();
			op = stack.Pop();
		} catch(Exception e){
			Console.WriteLine ("STACK HERE!");
			throw new SyntaxException (e.Message);
		}

		switch (op) {
		case "=":
			if (val1 == val2)
				return "WIN";
			else
				return "FAIL";
		default:
			throw new Exception ("Something went wrong in evaluate...");
		}
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
