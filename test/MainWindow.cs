using System;
using Gtk;
using test;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public partial class MainWindow: Gtk.Window
{	
	LexemeCreator lexer = new LexemeCreator(); //for lexical analysis
	List<Lexeme> lexemeList = new List<Lexeme>();
	List<Lexeme> allLex = new List<Lexeme> ();
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
		outputField.Buffer.Text = "";
		hasStarted = false;
		hasEnded = false;
		hasError = false;
		tokensListStore.Clear ();
		symbolTableListStore.Clear ();
		allLex.Clear ();
		table.Clear ();
		string sourceString = sourceText.Buffer.Text;
		string[] sourceLines = sourceString.Split (delimeter);
		for(int i=0 ; i< sourceLines.Length; i++) { //infinite loop
			line = sourceLines [i];
			lineField.Buffer.Text = (i+1) + ": " + line;
			try {
				lexemeList = lexer.process (line); //creates an array of lexemes
				parse (); //parses the lexemes
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
			allLex.AddRange(lexemeList);
			lexer.reset (); //resets the lexer
		}
		if(!hasEnded && !hasError) outputField.Buffer.Text += "\n" + "ERROR on line " + (sourceLines.Length+1) + ": Program is not closed properly!\n";
		if(hasEnded) lineField.Buffer.Text = "Done!";
	}

	public void parse()
	{ //processses the lexemes
		//Console.WriteLine("\nPARSING:");
		char[] delimeter = {' '};
		if (lexemeList.Count == 0)
			return;
		
		for(int i=0; i < lexemeList.Count; i++){
			Console.WriteLine (lexemeList.Count);
			if (lexemeList [i].getName ().Equals (Constants.STARTPROG)) {
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
						}
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
									table.Add (l1.getName (), new Value ("NOOB", "Untyped")); //makes the variable NOOB
								} else //else ITZ has no arguments
									throw new SyntaxException ("Expected constant or variable after ITZ.");
							}
						}
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
							return;
						} else //else the right side is neither a variable or a constant
							throw new SyntaxException ("Only variable or constants should be on left hand side of R");

					} else //else the left side is not a vairblae
						throw new SyntaxException ("Variable should be on left hand side of R. " + var.getName () + " is not a variable.");

					i++;
				} else if (lexemeList [i].getDescription ().Equals ("Variable Identifier")) { //checks if the keyword is KTHXBYE
					if (!table.ContainsKey (lexemeList [i].getName ())) //check if the variable is already declared
						throw new SyntaxException ("Variable " + lexemeList [i].getName () + " is not yet declared!");
				} else if (lexemeList [i].getName ().Equals (Constants.ENDPROG)) { //checks if the keyword is KTHXBYE
					hasEnded = true;
				} else if (lexemeList[i].getDescription ().Contains ("Operator")) {
					operatorList (lexemeList, ref i);
				} else
					throw new WarningException("Unexpected lexeme " + lexemeList[i].getName() + " found!");
			}
		}
		//Console.WriteLine("========");
	}

	private String operatorList(List<Lexeme> lexemeList, ref int index){
		string name = lexemeList [index].getName ();
		string result;
		if (index + 1 == lexemeList.Count)
			throw new SyntaxException (name + " has no arguments!");

		index++;
		if(name.Equals(Constants.CONCAT))
			result = concatString (lexemeList, ref index);
		else
			throw new WarningException("Unexpected lexeme " + name + " found!");
		
		return result;
	}

	private String concatString(List<Lexeme> lexemeList, ref int index){
		String result = "";
		bool hasEnded = false;
		for (int i = index; i < lexemeList.Count; i++) {
			if (hasEnded)
				throw new SyntaxException ("Unexpected " + lexemeList[i].getName() + " found!");
			else if (lexemeList [i].getDescription ().EndsWith ("constant"))
				result += lexemeList [i].getName();
			else if (lexemeList[i].getDescription().EndsWith("Variable Identifier")){
				if(table.ContainsKey(lexemeList[i].getName())){
					string varname = lexemeList[i].getName();
					if(table[varname].getValue().Equals("NOOB"))
						continue;
					else
						result += table[varname].getValue();
				} else
					throw new SyntaxException("Variable " + lexemeList[index].getName() + " is not declared!");
			} else if(lexemeList[i].getName().Equals(Constants.MKAY)){
				hasEnded = true;
			} else if(lexemeList[i].getName().Equals(Constants.AN) || lexemeList[i].getName().Equals("\"")){
				continue;
			} else
				throw new SyntaxException( "Unexpected " + lexemeList[i].getName() + " found!");
		}
		index = lexemeList.Count;
		return result;
	}

	public void UpdateTables(){
		foreach (Lexeme l in allLex) {
			tokensListStore.AppendValues (l.getName(), l.getDescription());
		}
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
		UpdateTables ();
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
}
