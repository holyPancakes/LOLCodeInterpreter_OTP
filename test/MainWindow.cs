using System;
using Gtk;
using test;
using System.Collections.Generic;

public partial class MainWindow: Gtk.Window
{	
	LexemeCreator lexer = new LexemeCreator(); //for lexical analysis
	List<Lexeme> lexemeList = new List<Lexeme>();
	List<Lexeme> allLex = new List<Lexeme> ();
	String line;
	char[] delimeter = {'\n'};

	private Dictionary<String, Value> table = new Dictionary<String, Value>(); //symbol table
	private Boolean hasEnded; //checks if the program already ended using KTHXBYE

	Gtk.ListStore tokensListStore;
	Gtk.ListStore symbolTableListStore;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
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


	}

	public void Interpret(){
		hasEnded = false;
		tokensListStore.Clear ();
		symbolTableListStore.Clear ();
		allLex.Clear ();
		table.Clear ();
		string sourceString = sourceText.Buffer.Text;
		string[] sourceLines = sourceString.Split (delimeter);
		int lineNumber = 1;
		try{
			lexemeList = lexer.process (sourceLines[0]); //creates an array of lexemes
			parseHAI (); //parses the lexemes
			lexer.reset ();
			lineNumber ++;
		} catch (Exception e) { //if something went wrong, prints the error on screen
			outputField.Buffer.Text += "\n ERROR " + e.Message + "\n";
			//return;
		}
		allLex.AddRange(lexemeList);
		for(int i=1 ; i< sourceLines.Length; i++) { //infinite loop
			line = sourceLines [i];

			try {
				lexemeList = lexer.process (line); //creates an array of lexemes
				parse (); //parses the lexemes
			} catch (Exception e) { //if something went wrong, prints the error on screen
				outputField.Buffer.Text += "\n" + "ERROR on line " + (i+1)+ e.ToString()+ "\n";
				break;
			}
			allLex.AddRange(lexemeList);
			lexer.reset (); //resets the lexer
		}
	}

	public void parseHAI()
	{ //checks if the program starts with HAI
		Lexeme top = lexemeList[0];
		if (!top.getName ().Equals ("HAI")) {
			throw new Exception (" All programs should start with HAI!");
		}
	}

	public void parse()
	{ //processses the lexemes
		//Console.WriteLine("PARSING:");
		char[] delimeter = {' '};
		for(int i=0; i < lexemeList.Count; i++){
			if(hasEnded)
			{
				throw new Exception("Program already ended!");
			}
			else
			{
				if(lexemeList[i].getName().Equals("VISIBLE")){ //checks if the keyword is VISIBLE
					if(lexemeList.Count - i >= 2){ //checks if VISIBLE has arguments
						Lexeme l = lexemeList[i+1]; //gets the next lexeme
						if(l.getDescription().EndsWith("YARN constant")){ //checks if it is a string constant
							outputField.Buffer.Text += l.getName()+ "\n"; //prints the string
						}else if(l.getDescription().Equals("Variable Identifier")){
							if(!table.ContainsKey(l.getName())) //checks if the variable is already declared
								throw new Exception("Variable identifier '" + l.getName() + "' is not yet declared.");
							Value val = table[l.getName()]; //gets the value of the variable
							outputField.Buffer.Text += val.getValue()+ "\n"; //prints the value of the variable
						}else throw new Exception(l.getName() + " is not printable!"); //error that a value is not printable
						i++; //increments the index
					}else //else VISIBLE has no arguments
						throw new Exception("VISIBLE has no arguments!");
				}else if(lexemeList[i].getName().Equals("I HAS A")){ //checks if the keyword is I HAS A
					if(lexemeList.Count - i == 2){ //checks if I HAS A has arguments
						Lexeme l = lexemeList[i+1]; //gets the next lexeme
						if(l.getDescription().Equals("Variable Identifier")){ //checks if it is a variable identifier
							if(table.ContainsKey(l.getName())) //checks if the variable is already in the table
								throw new Exception("Variable identifier " + l.getName() + " already exists.");
							table.Add(l.getName(), new Value("NOOB", "Untyped")); //makes the variable NOOB
						}
						i++; //increments the index
					}
					else if(lexemeList.Count > 2){ //checks if the statement also starts with a value
						Lexeme l1 = lexemeList[i+1]; //gets the next lexeme
						if(l1.getDescription().Equals("Variable Identifier")){ //checks if it is a variable identifier
							if(table.ContainsKey(l1.getName())) //checks if the variable is in the table
								throw new Exception("Variable " + l1.getName() + " already exists.");
							Lexeme l2 = lexemeList[i+2]; //gets the next lexeme of next lexeme
							if(l2.getDescription().Equals("Assigns value after declaration.")){ //checks if it is ITZ
								Lexeme l3 = lexemeList[i+3]; //gets the argument of ITZ
								if(l3.getDescription().EndsWith("constant")){ //checks if the argument is contstant
									String[] type = l3.getDescription().Split(delimeter); //gets the datatype of the value
									table.Add(l1.getName(), new Value(l3.getName(), type[0])); //puts it to table
								}else if(table.ContainsKey(l3.getName())){ //checks if the argument is a variable and it is initialized
									table.Add(l1.getName(), table[l3.getName()]); //copies the value and puts it to the table
								}else //else ITZ has no arguments
									throw new Exception("Expected constant or variable after ITZ.");
							}
						}
						i+=3;
					}
					else{ //else throws exception
						throw new Exception("I HAS A has no arguments!");
					}
				}else if(lexemeList[i].getName().Equals("R")){ //checks if the keyword is R
					Lexeme var = lexemeList[i-1]; //gets the left and right lexemes of R
					Lexeme value = lexemeList[i+1];
					if(var.getDescription().Equals("Variable Identifier")){ //checks if the left side is a variable
						if(value.getDescription().EndsWith("constant")){ //checks if the right side is a constant
							String[] type = value.getDescription().Split(delimeter); //gets the dataype
							Value old = table[var.getName()]; //gets the old datatype of the variable

							table.Add(var.getName(), new Value(value.getName(), type[0])); //puts new value to table
						}else if(value.getDescription().Equals("Variable Identifier")){ //checks if the right side is a variable
							if(!table.ContainsKey(value.getName())) //check if the variable is already declared
								throw new Exception("Variable " + value.getName() + " is not yet declared!");
		
							table.Add(var.getName(), table[value.getName()]); //changes the value of the variable
						}else //else the right side is neither a variable or a constant
							throw new Exception("Only variable or constants should be on left hand side of R");

					}else //else the left side is not a vairblae
						throw new Exception("Variable should be on left hand side of R. " + var.getName() + " is not a variable.");

					i++;
				}else if(lexemeList[i].getName().Equals("KTHXBYE")){ //checks if the keyword is KTHXBYE
					hasEnded = true;
				}
			}
		}
		//Console.WriteLine("========");
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

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	protected void runProgramClick (object sender, EventArgs e)
	{
		Interpret ();
		UpdateTables ();
	}

}
