using Gtk;
using test;
using Pango;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Media;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
public partial class MainWindow: Gtk.Window
{	
	private List<Dictionary<String, Value>> allTable; //all of the symbol table including the variable scope
	private List<Lexeme> lexemeList = new List<Lexeme>(); //list of lexemes
	private Stack<Value> stack = new Stack<Value> (); //stack used for operations
	private Stack<Value> stackArity = new Stack<Value>(); //stack used for arity
	private Dictionary<String, Value> table; //current used in a code block

	private Boolean hasEnded; //when the program already ended
	private Boolean hasStarted; //when the program already started
	private Boolean hasError; //when an error is found in the program

	private Gtk.ListStore tokensListStore; //for the tree view in the right side of the GUI used for lexemes
	private Gtk.ListStore symbolTableListStore; //for the tree view in the right side of the GUI used for symbol table

	private TextView inputTextView; //console of the program

	public MainWindow (): base (Gtk.WindowType.Toplevel){ //constructor of the window
		Build ();

		//for initializing lexeme list tree view in the window
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

		//for initializing the tree view for symbol table
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

		//initializes the essential part of coding a LOLCODE program
		sourceText.Buffer.Text = Constants.STARTPROG + "\n\n" + Constants.ENDPROG;

		//sets the font of textview of source code
		sourceText.ModifyFont(FontDescription.FromString("Courier 12"));
		sourceText.Buffer.PlaceCursor (sourceText.Buffer.GetIterAtLine(1));

		//sets the font, fg color, and bg color of the console
		outputField.ModifyFont(FontDescription.FromString("Courier 12"));
		outputField.ModifyBase(StateType.Normal, new Gdk.Color(0x00, 0x00, 0x00));
		outputField.ModifyText(StateType.Normal, new Gdk.Color(0xFF, 0xFF, 0xFF));
		sourceText.GrabFocus (); //the sourcetext will grab the focus

		//initializes the symbol table
		allTable = new List<Dictionary<String, Value>>();
	}

	public void Interpret(){ //interprets the code
		LexemeCreator lexer = new LexemeCreator(); //creates the creator of lexemes
		char[] delimeter = {'\n'}; //sets the line delimiter
		int i = 0; //initializes the counter
		string sourceString = sourceText.Buffer.Text; //gets the source code
		string[] sourceLines = sourceString.Split (delimeter); //splits the source code into lines

		outputField.Buffer.Text = ""; //clears the console

		//initializes the boolean values
		hasStarted = false;
		hasEnded = false;
		hasError = false;

		//clears all the stacks, lists, dictionaries
		tokensListStore.Clear ();
		symbolTableListStore.Clear ();
		lexemeList.Clear ();
		allTable.Clear ();
		stack.Clear ();
		stackArity.Clear ();

		//shows the source code in the actual console of the program for debugging purposes
		Console.WriteLine ("==========");
		Console.WriteLine (sourceText.Buffer.Text);
		Console.WriteLine ("==========");
		
		try {
			lineField.Buffer.Text = "Creating Lexemes..."; //shows that lexemes are being created
			lexemeList = lexer.process (sourceLines); //creates lexemes and puts the list of lexemes to lexemeList
			processLexemes(); //process the lexemes. converts all \t to :>
			UpdateLexemes(lexemeList); //updates the tree view for lexemes
			lineField.Buffer.Text = "Running the Lexemes..."; //shows that lexemes are now used to interpret the program
			parse (ref i, Constants.STARTPROG); //runs the lexemes
		} catch (SyntaxException e) { //when syntax error is found
			hasError = true; //makes the boolean true
			outputField.Buffer.Text += "\n" + "ERROR: " + e.Message + "\n"; //shows the error in the console
			Console.WriteLine ("\n" + "ERROR: " + e.Message + "\n" + e.StackTrace + "\n"); //writes the error and where it comes from in the actual console
			return; //exits the function
		} catch (Exception e){ //when something bad really happened in the program
			//shows the error message and when it came from in the console and the actual console
			outputField.Buffer.Text += "\n" + "CRASHED: " + e.Message + "\n" + e.StackTrace+ "\n"; 
			Console.WriteLine ("\n" + "CRASHED: " + e.Message + "\n" + e.StackTrace + "\n");
			return; //exits the function
		}
		//shows the error when there are no errors and the program is not closed properly
		if(!hasEnded && !hasError) outputField.Buffer.Text += "\n" + "ERROR: Reach end of file. Did not found " + Constants.ENDPROG + "\n";
		else if(hasEnded) lineField.Buffer.Text = "Done!"; //else shows that the program is done interpreting
	}

	public void processLexemes(){ //converts all instances of \t to :>
		for (int i = 0; i < lexemeList.Count; i++) {
			string name = lexemeList [i].getName ();
			if (name.Contains ("\t"))
				lexemeList [i] = new Lexeme (
					name.Replace ("\t", ":>"),
					lexemeList [i].getDescription ()
				);
		}
	}

	//i is the index in lexeme list, fromWhere is who called this function (HAI if from the start, YA RLY if if-else, OMG if switch-case, etc.)
	public string parse(ref int i, string fromWhere){
		//initializes the flags
		bool nextCommand = false; //when a command is done and next command is needed
		bool broken = false; //when GTFO is encountered in the program
		//separator used
		char[] space = { ' ' };
		string desc; //lexeme description
		string name; //lexeme name

		allTable.Add(new Dictionary<string, Value> ()); //adds a new symbol table to all of the symbol table
		table = allTable [allTable.Count-1]; //make table equate to allTable

		if (fromWhere == Constants.STARTPROG) //initialize the implicit variable if HAI called this function
			allTable [0].Add (Constants.IMPLICITVAR, new Value (Constants.NULL, Constants.NOTYPE));

		for(; i < lexemeList.Count; i++){ //for each lexemes in lexemeList
			desc = lexemeList [i].getDescription ();
			name = lexemeList [i].getName ();

			if (desc.Contains ("comment")) { //check if it is a comment
				if (name == Constants.ENDCOMMENT || name == Constants.ONELINE) {
					nextCommand = true;
				}
			} else if (desc.Contains ("break")) { //check if it is a break (\n or ,)
				nextCommand = false; //next command can now be called
			} else if (nextCommand) { //if a command is done and a keyword is found again
				throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			}else if (name == Constants.STARTPROG) { //when the start ptogram is found
				if(hasStarted)  //check if start program is found earlier
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.STARTPROG));
				hasStarted = true; //else starts the program
			} else if (!hasStarted) //if the program is not yet started
				throw new SyntaxException (WarningMessage.noStart());
			else if(hasEnded) //if the program is already ended
				throw new SyntaxException(WarningMessage.alreadyEnd());
			else{
				if (name == Constants.PRINT) { //if print keyword is found
					printParse (ref i); //parse using the visible command
					nextCommand = true;
				} else if (name == Constants.SCAN) { //if scan keyword is found
					scanParse (ref i); //parse using the gimmeh command
					nextCommand = true;
				} else if (name == Constants.VARDEC) { //if variable declaration is found
					varDecParse (ref i); //parse using the I HAS A
					nextCommand = true;
				} else if (name == Constants.ASSIGN) { //if assign is found
					assignParse (ref i); //parse using R
					nextCommand = true;
				} else if (desc == Constants.VARDESC) { //if a variable is found
					int index = findVarName(name); //check if the variable is already initialized
					if (index == -1) //throw semantic error when the variable is already declared
						throw new SyntaxException (WarningMessage.varNoDec (name));
					else if (lexemeList [i + 1].getName () == Constants.ASSIGN || 
						lexemeList [i + 1].getName () == Constants.VARCAST) 
						//if variable typecasting or asignment operation is found, do nothing
						continue;
					else { //else put the value to the implicit variable
						allTable[0][Constants.IMPLICITVAR] = allTable[index][name];
						nextCommand = true;
					}
				} else if (name == Constants.ENDPROG) { //if KTHXBYE is found
					hasEnded = true;
					nextCommand = true;
				} else if (desc.Contains ("Operator")) { //if an operator is found
					string value = operatorList (lexemeList, ref i); //evalueate and get the result
					string type = returnType (value); //check what is the most appropriate type for the result

					allTable[0][Constants.IMPLICITVAR] = new Value (value, type); //put the value to the implicit variable
					nextCommand = true;
				} else if (desc.Contains ("constant")) { //if constant is found
					if (lexemeList [i - 1].getName () != Constants.CASE) { //if OMG is not before the constant
						string[] type = desc.Split (space);
						allTable[0][Constants.IMPLICITVAR] = new Value (name, type [0]); //put the value to the implicit variable
					}
					nextCommand = true;
				} else if(desc.Contains("Typecasts")){ //check if it is a type casting keyword
					typeParse (ref i); //parse the command
					nextCommand = true;
				} else if (name == Constants.SWITCH) { //check if it is a switch case keyword
					switchParse (ref i); //parse the switch-case
					nextCommand = true;
					i--;
				} else if (name == Constants.CONDITION) { //check if it is an if-else statement
					ifParse (ref i); //parse the if-else
					nextCommand = true;
					i--;
				} else if (name == Constants.STARTLOOP) { //check if it is a loop
					loopParse (ref i); //parse the loop
					nextCommand = true;
					i--;
				} else if (name == Constants.END_IF || 
					name == Constants.BREAK || 
					name == Constants.ELSE ||
					name == Constants.ENDLOOP) { //check if the keyword ends something
					if (fromWhere == Constants.SWITCH &&
						(name == Constants.END_IF || name == Constants.BREAK)) {
						break; //check if it will break in switch case
					} else if (fromWhere == Constants.IF && 
						(name == Constants.END_IF || name == Constants.ELSE)) {
						break;//check if it will break in if else
					} else if (fromWhere == Constants.STARTLOOP && 
						(name == Constants.ENDLOOP || name == Constants.BREAK)) {
						if (name == Constants.BREAK)
							broken = true;
						break;//check if it will break in loop
					} else 
						throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
				} else if (name == Constants.CASE || name == Constants.DEFAULT) { //check if it is OMG or OMGWTF
					if(fromWhere == Constants.STARTPROG) 
						throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
				}else
					throw new WarningException(WarningMessage.unexpectedLexeme(name));
			}
			UpdateTables(); //updates the tree view of symbol table
		}

		if (allTable.Count > 1) { //deletes the local variable
			allTable.Remove (table);
			table = allTable [allTable.Count - 1];
		}
		UpdateTables(); //updates the tree view of symbol table

		if (broken) //if GTFO is found, returns it
			return Constants.BREAK;
		else return ""; //else returns nothing
	}

	private void typeParse(ref int i){ //for typecasting
		string name = lexemeList [i].getName (); //gets the lexeme name
		int ival; //integer value
		double numbarval; //float value for variable
		float fval; //flaot value for the operation
		string stval; //string value
		string troof; //troof value
		string val; //value
		string type; //type
		string a; //temporary variable
		int index; //index of the lexemeList
		string desc; //description of lexeme
		string result; //result of the operation
		string checker;
		int k; //temporary variable

		if (name.Equals (Constants.EXPCAST)) { //if MAEK is found
			i++; //update the index
			desc = lexemeList [i].getDescription (); //get the descritpion
			if (!desc.Contains ("Operator")) { //check if it is not an operator
				val = lexemeList [i].getName ();
				index = findVarName (val);
				checker = table [val].getValue ();
				i++;
				a = lexemeList [i].getName ();
			} else { //marks the variabless that not an operator is found
				val = "";
				index = 0;
				checker = "";
				a = "";
			}
			i++; //update the index
			for (k = i; lexemeList [k].getName () != Constants.EOL; k++); //save the end of line of the command

			type = lexemeList [k - 1].getName (); //get the data type

			if (checker == Constants.NULL) { //if NOOB is found, throw error
				throw new SyntaxException (WarningMessage.varNoVal (val));
			} else if (!a.Equals (Constants.A)) { //if lexeme name is not equal to keyword A
				throw new SyntaxException (WarningMessage.unexpectedLexeme (a));
			} else if (desc.Contains ("Operator")) { //if operator is to be typecasted
				i = i - 1; //decrement the index
				result = operatorList (lexemeList, ref i); //parse the operator
				i = i + 2; //increment the index
				if (type == Constants.INT) { //if type is an integer
					if (int.TryParse (result, out ival)) { //try parsing the result to integer
						allTable [0] [Constants.IMPLICITVAR] = new Value (ival.ToString (), Constants.INT);
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.FLOAT) { //if type is a float
					if (float.TryParse (result, out fval)) { //try parsing the result to float
						allTable [0] [Constants.IMPLICITVAR] = new Value (fval.ToString (), Constants.FLOAT);
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.STRING) { //if type is a string
					allTable [0] [Constants.IMPLICITVAR] = new Value (result, Constants.STRING); //just save the value
				} else if (type == Constants.BOOL) {//if type is a boolean
					if (Constants.BOOLVAL.Match (result).Success) { //check if it matches the pattern for TROOF values
						allTable [0] [Constants.IMPLICITVAR] = new Value (result, Constants.BOOL);
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				}
			} else { //else if the expression is an integer
				if (type == Constants.INT) {  //check if type is an integer
					//check if the value is a float or an integer
					if (Constants.INTVAL.Match (table [val].getValue ()).Success || Constants.FLOATVAL.Match (table [val].getValue ()).Success) {
						if (Constants.INTVAL.Match (table [val].getValue ()).Success) { //if it is an integer parse it to integer
							ival = Int32.Parse (table [val].getValue ());
						} else { //else round it off
							numbarval = double.Parse (table [val].getValue ());
							ival = (int)Math.Round (numbarval);
						}	
						allTable [index] [val] = new Value (ival.ToString (), Constants.INT); //save the new value
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.FLOAT) { //if it is a float
					//check if it matches a float value or an integer value
					if (Constants.FLOATVAL.Match (table [val].getValue ()).Success || Constants.INTVAL.Match (table [val].getValue ()).Success) {
						numbarval = float.Parse (table [val].getValue ()); //parse the float
						allTable [index] [val] = new Value (numbarval.ToString (), Constants.FLOAT); //save the value
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.STRING) { //if it is a string
					stval = table [val].getValue ().ToString (); //get the value and save it
					allTable [index] [val] = new Value (stval, Constants.STRING);
				} else if (type == Constants.BOOL) { //it it is a TROOF
					if (Constants.BOOLVAL.Match (table [val].getValue ()).Success) { //check if the value matches the pattern for TROOF value
						troof = table [val].getValue ().ToString (); //get the value
						allTable [index] [val] = new Value (troof, Constants.BOOL); //save the value
					} else { //else throw an error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				}
			}
		} else if (name.Equals (Constants.VARCAST)) { //else if it is an IS NOW A
			val = lexemeList [i - 1].getName (); //get the value
			i++;
			type = lexemeList [i].getName (); //get the datatype to be typcasted into
			index = findVarName (val); //find the variable

			if (index == -1) { //check if the variable name exists
				throw new SyntaxException (WarningMessage.varNoDec (val));
			}
			else if (allTable [index][val].getValue () == "NOOB") { //check if the variable has value
				throw new SyntaxException (WarningMessage.varNoVal (val));
			} else {
				if (type == Constants.INT) { //check if typecasted into int
					if (Constants.INTVAL.Match (table [val].getValue ()).Success || Constants.FLOATVAL.Match (table [val].getValue ()).Success) {
						if (Constants.INTVAL.Match (table [val].getValue ()).Success) { //parse it to int
							ival = Int32.Parse (table [val].getValue ());
						} else { //else if it is a float, round it off
							numbarval = double.Parse (table [val].getValue ());
							ival = (int)Math.Round (numbarval);
						}	
						allTable [index] [val] = new Value (ival.ToString (), Constants.INT);					
					} else {
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.FLOAT) { //check if typecasted into float
					if (Constants.FLOATVAL.Match (table [val].getValue ()).Success || Constants.INTVAL.Match (table [val].getValue ()).Success) {
						numbarval = float.Parse (table [val].getValue ()); //parse it to float
						allTable [index] [val] = new Value (numbarval.ToString (), Constants.FLOAT); //save the value
					} else {
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				} else if (type == Constants.STRING) { //if string
					stval = table [val].getValue ().ToString ();
					allTable [index] [val] = new Value (stval, Constants.STRING); //save the value
				} else if (type == Constants.BOOL) { //if bool
					if (Constants.BOOLVAL.Match (table [val].getValue ()).Success) { //check if it matches the TROOF pattern
						troof = table [val].getValue ().ToString ();
						allTable [index] [val] = new Value (troof, Constants.BOOL); //save the value
					} else { //else throw error
						throw new SyntaxException (WarningMessage.noConverto (val, type));
					}
				}
			}
		}
	}

	private void ifParse(ref int i){ //parses the if-else statement
		int elseindex = 0; //index of else
		int ifIndex = 0; //index of if
		int index = 0; //actual index
		string value = allTable[0][Constants.IMPLICITVAR].getValue (); //gets the value of IT
		string name = lexemeList [i].getName (); //gets the lexeme name
		string desc = lexemeList [i].getDescription (); //gets the lexeme description

		for (;  name != Constants.END_IF; i++) { //for each lexeme until OIC
			desc = lexemeList [i].getDescription ();
			name = lexemeList [i].getName ();
			if (!desc.Contains ("constant")) {
				if (name == Constants.IF) { //finds the index of IF
					ifIndex = i + 1;
				} else if (name == Constants.ELSE) { //and infex of else
					elseindex = i + 1;
				} else if (name == Constants.ENDPROG) { //throw error if OIC is not found
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDPROG));
				}
			}
		}
		if (ifIndex == 0) //throw error if IF is not found
			throw new SyntaxException (WarningMessage.noIF ());
		
		index = (value == Constants.TRUE) ? ifIndex : elseindex; //gets the if index if the value is true, else gets the else index

		if (index == 0) //returns if else index is not found and if index is not true
			return;
		
		parse (ref index, Constants.IF); //parses the code block inside
	}

	private void loopParse(ref int index){//parses the loop
		string loopLabel; //loop label
		string result; //result of comparison
		string varname; //counter in loop

		bool increment; //UPIN or NERFIN
		bool tilWIN; //TIL or WILE

		int tableIndex; //index of the counter in symbol table
		int condIndex; //index of the condition
		int start; //start of interpreting the loop
		int afterLoop = 0; //index after the loop

		if (lexemeList [++index].getDescription () == Constants.VARDESC) { //if the label of the loop fits the regex of variable
			loopLabel = lexemeList [index].getName (); //gets the looplabel
		} else { //else throw error
			throw new SyntaxException(WarningMessage.unexpectedLexeme(lexemeList[index].getName()));
		}
		
		if (lexemeList [++index].getName () == Constants.INC) { //checks if the counter should be incremented
			increment = true;
		} else if (lexemeList [index].getName () == Constants.DEC) { //or decremented
			increment = false;
		} else { //else throw error
			throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [index].getName ()));
		}

		if (lexemeList [++index].getName () == Constants.YR) { //else check if YR is found after
			varname = lexemeList [++index].getName (); //gets the variable name
			string desc = lexemeList [index].getDescription (); //gets the variable description
			tableIndex = findVarName (varname); //finds the variable in the symbol table
			if (desc != Constants.VARDESC) //check if the lexeme is not a variable
				throw new SyntaxException (WarningMessage.unexpectedLexeme (varname));
			else if (tableIndex == -1) //check if the variable is not found in the symbol table
				throw new SyntaxException (WarningMessage.varNoDec (varname)); 
			else if (allTable [tableIndex] [varname].getType () != Constants.INT) //check if the variable is not an integer value
				throw new SyntaxException (WarningMessage.cannotConvert (allTable [tableIndex] [varname].getValue (), Constants.INT));
		} else { //else throw an error if YR is not found
			throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [index].getName ()));
		}
			
		if (lexemeList [++index].getName () == Constants.LOOPCONDWIN) { //check if loop should continue until it is a WIN
			tilWIN = true;
		} else if (lexemeList [index].getName () == Constants.LOOPCONDFAIL) { //or until it is a FAIL
			tilWIN = false;
		} else { //else throw an error
			throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [index].getName ()));
		}

		condIndex = ++index; //gets the index of condition
		if(lexemeList[index].getDescription().Contains("Operator")){ //check if it is indeed an operator
			result = operatorList (lexemeList, ref index); //get the result
			string type = returnType (result); //check if the result is a TROOF value
			if (type != Constants.BOOL) //if not throw an error
				throw new SyntaxException (WarningMessage.cannotConvert (result, Constants.BOOL));
		} else { //if not an operator throw an error
			throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [index].getName ()));
		}

		//get the end of the loop
		while (lexemeList [index].getName () != Constants.ENDLOOP) {
			if (lexemeList [index].getName () == Constants.EOL && afterLoop == 0)
				afterLoop = index; //save it to a variable
			index++;
		}

		if(lexemeList[index+1].getName() != loopLabel) //if the ending loop label does not match the initial loop label, throw an error
			throw new SyntaxException(WarningMessage.unexpectedLexeme(lexemeList[index+1].getName()));

		start = afterLoop;

		//evaluate the loop
		while ((!tilWIN && result == "FAIL") || (!tilWIN && result == "WIN")) {
			parse (ref start, Constants.STARTLOOP); //parse the code block

			Value val = allTable [tableIndex] [varname]; //get the value
			int value = int.Parse (val.getValue ());
			//increment or decrement the value
			if (increment)
				value++;
			else
				value--;
			//save it to symbol table
			allTable [tableIndex] [varname] = new Value (Convert.ToString (value), val.getType ());

			//check the condition again
			start = condIndex;
			result = operatorList (lexemeList, ref start);
		}

		//update the index and go back to parse function
		index+=2;
	}

	private void switchParse(ref int i){ //parses the switch case
		Dictionary<Value,int> cases = new Dictionary<Value,int> (); //gets the case value as key and index as value
		int index = 0; //index for the lexeme list
		string name = lexemeList [i].getName (); //lexeme name
		string desc = lexemeList [i].getDescription (); //lexeme desc
		string itValue = allTable[0][Constants.IMPLICITVAR].getValue (); //value of it
		string itType = allTable[0][Constants.IMPLICITVAR].getType (); //type of it
		Value omgwtf = new Value (Constants.DEFAULT, "DEFAULT");

		for (; name != Constants.END_IF; i++) { //for each lexemes until OIC
			name = lexemeList [i].getName (); //get the lexeme name
			desc = lexemeList [i].getDescription (); //get the lexeme description
			if (!desc.Contains ("constant")) {
				if (name == Constants.CASE) { //check if the name is OMG
					//saves it to the dictionary
					if (lexemeList [i + 1].getName ().Equals ("\"")) {
						cases.Add (new Value(lexemeList [i + 2].getName (), Constants.STRING), i + 4);
					} else if (lexemeList [i + 1].getDescription ().Contains ("constant")) {
						string type = (Constants.INTVAL.Match (lexemeList [i + 1].getName ()).Success) ? Constants.INT : Constants.FLOAT;
						cases.Add (new Value(lexemeList [i + 1].getName (), type), i + 2);
					} else {
						throw new SyntaxException (WarningMessage.unexpectedLexeme (lexemeList [i + 1].getName ()));
					}
				} else if (name == Constants.ENDPROG){ //if KTHXBYE is found throw an error
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.ENDPROG));
				} else if (name == Constants.DEFAULT) { //saves the default case to the dictionary
					cases.Add (omgwtf, i + 1);
				}
			}
		}

		foreach (Value g in cases.Keys) { //for each value in cases
			string val = g.getValue ();
			string type = g.getType ();
			if (itValue == val && itType == type) { //if it matches
				index = cases [g]; //get the index
			}
		}

		if (index == 0) { //if none matches get the default case
			if (cases.ContainsKey (omgwtf))
				index = cases [omgwtf];
			else //if no default case exit the funciton
				return;
		}

		parse (ref index, Constants.SWITCH); //parse the code block
	}

	private void printParse(ref int i){ //parses the printing
		//check if VISIBLE has arguments
		if (lexemeList [i + 1].getDescription ().Contains ("break") || lexemeList [i + 1].getName () == Constants.NONEWLINE)
			throw new SyntaxException (WarningMessage.noArguments (Constants.PRINT));

		//for each lexeme until the '!' or the break
		for (i++; 
			!lexemeList [i].getDescription ().Contains ("break") && 
			lexemeList [i].getName () != Constants.NONEWLINE; 
			i++) 
		{
			string name = lexemeList [i].getName (); //get the lexeme name
			string desc = lexemeList [i].getDescription (); //get the lexeme description
			string toWrite; //string to be written

			if (desc.Contains ("constant") || desc.Contains ("Operator") ||
			    desc == Constants.VARDESC || name == "\"") { //if it is printable
				if (name == "\"") { //if it is a string
					Lexeme yarn = lexemeList [i + 1]; //get the string
					toWrite = yarn.getName (); //saves the value
					i += 2; //updates the index
				} else if (desc.Contains ("constant")) { //if it is a constant
					toWrite = name; //get the value
				} else if (desc == Constants.VARDESC) { //if it is a variable
					int index = findVarName (name); //finds the variable name
					if (index == -1) //if not declared throw an error
						throw new SyntaxException (WarningMessage.varNoDec (name));
					Value val = allTable [index] [name]; //gets the value of the variable
					if (val.getValue () == Constants.NULL) //checks if the variable has value
						throw new SyntaxException (WarningMessage.cannotNull ());
					toWrite = val.getValue (); //gets the value of the variable
				} else if (desc.Contains ("Operator")) { //if it is an operator
					toWrite = operatorList (lexemeList, ref i); //parse the operator
					if (!lexemeList [i].getDescription ().Contains ("constant") &&
					   lexemeList [i].getDescription () != Constants.VARDESC)
						i--;
				} else //throw an error if it is not printable
					throw new SyntaxException (WarningMessage.notPrintable (name));
				printString (toWrite, ref i); //print the string
			}
		}

		if (lexemeList [i].getName () != Constants.NONEWLINE) { //check if new line should be printed
			outputField.Buffer.Text += "\n";
			i--;
		}
	}

	private void printString(string print, ref int i){ //prints the string and substitutes the escape characters
		bool escFound = false;
		string esc = "";

		foreach (char c in print) { //for each character to be printed
			if (escFound) { //if an escape character is already found
				esc += c; //adds it to escape string
				if (processEsc (esc)) { //precess it and checks if it means something
					escFound = false; //if successful turns off the trigger
					esc = ""; //resets the escape string
				}
			} else if (c == ':') { //if escape character is found
				esc += c; //adds it to the escape string
				escFound = true; //turns on the trigger
			} else //else print the character to the console
				outputField.Buffer.Text += c;
		}

		if (escFound) { //if after all of that the escape character has no meanining
			if (!processEsc (esc)) //try processing it. if no luck throw an error
				throw new SyntaxException (WarningMessage.unrecognizedEscChar (esc));
		}
	}

	private bool processEsc(string esc){ //process the escape string
		//escape string fro LOLCODE to C#
		Dictionary<string, string> lol2C = new Dictionary<string, string> () {
			{ ":)", "\n" },
			{ ":>", "\t" },
			{ ":O", "" },
			{ ":\"", "\"" },
			{ "::", ":" }
		};

		//pattern of other escape string
		Regex hex = new Regex ("^:\\([0-9A-Fa-f]+\\)$", RegexOptions.Compiled);
		Regex varPrint = new Regex ("^:{[A-Za-z][A-Za-z0-9_]*}$");

		string hexCode = "";
		string varname = "";

		if (lol2C.ContainsKey (esc)) { //if the escape character is found
			outputField.Buffer.Text += lol2C [esc]; //print the equivalent character
			if (esc == ":O") //if it is a beep play a sound
				SystemSounds.Beep.Play ();
			return true;
		} else if (hex.IsMatch (esc)) { //if it is a escape character for a unicode character
			//get the hexcode
			hexCode = esc.Replace (":(", "");
			hexCode = hexCode.Replace (")", "");
			//get the equivalent character
			char ch = (char)Convert.ToInt32 (hexCode, 16);
			//print it to the console
			outputField.Buffer.Text += ch.ToString ();
			return true;
		} else if (varPrint.IsMatch (esc)) { //if it is a variable
			//get the variable name
			varname = esc.Replace (":{", "");
			varname = varname.Replace ("}", "");

			//find the variable name
			int i = findVarName (varname);
			if (i == -1) //throw an error if it is not found
				throw new SyntaxException (WarningMessage.varNoDec (varname));

			//print the variable name value to the console.
			outputField.Buffer.Text += allTable [i] [varname].getValue ();
			return true;
		}

		return false;
	}

	private void scanParse(ref int i){ //parses the GIMMEH
		string name = lexemeList [i+1].getName (); //get the lexeme name
		string desc = lexemeList [i+1].getDescription (); //get the lexeme description

		if (desc == Constants.VARDESC) { //if it is a variable
			int index = findVarName (name); //find the variable
			if (index != -1) { //if it is inisialized
				Dialog inputPrompt = null;
				ResponseType response = ResponseType.None;

				try {
					//create a prompt
					inputPrompt = new Dialog (Constants.SCAN, this, 
						DialogFlags.DestroyWithParent | DialogFlags.Modal, 
						"OK", ResponseType.Yes);

					//set the size
					inputPrompt.Resize (300, 50);
					//set the layout
					inputPrompt.VBox.Add (inputTextView = new TextView ());
					//show the prompt
					inputPrompt.ShowAll ();
					//get the response
					response = (ResponseType)inputPrompt.Run ();
				} finally {
					if (inputPrompt != null) {
						if (response == ResponseType.Yes) {
							string input = inputTextView.Buffer.Text; //get the input
							Value val = new Value (input, Constants.STRING); //store it to the variable as YARN
							allTable[index][lexemeList [i + 1].getName ()] = val; 
						}
						inputPrompt.Destroy ();
					}
				}
			} else //else the variable is not yet declared
				throw new SyntaxException (WarningMessage.varNoDec (name));
		} else //else GIMMEH has no arguments
			throw new SyntaxException (WarningMessage.noArguments (Constants.SCAN));
		i++;
	}

	private void varDecParse(ref int i){ //parses the variable declaration
		string name = lexemeList [i+1].getName (); //get the lexeme name
		string desc = lexemeList [i+1].getDescription (); //get the lexeme description
		char[] spaces = { ' ' };

		if (lexemeList[i+2].getName() != Constants.STARTINIT) { //check if ITZ is not found
			if (desc == Constants.VARDESC) { //check if the next lexeme is a variable
				if (findVarName (name) != -1) //if the variable is not found throw an error
					throw new SyntaxException (WarningMessage.varYesDec (name));
				//add the variable to table with NOOB as value and Untyped as datatype
				table.Add (name, new Value (Constants.NULL, Constants.NOTYPE));
			} else //throw an error if it is not a variable
				throw new SyntaxException (WarningMessage.expectedWord (Constants.VARDESC, Constants.VARDEC));
			i++; //update the variable
		} else { //if ITZ is found
			if (desc == Constants.VARDESC) { //check if the next lexeme is a variable
				if (findVarName (name) != -1) //if the variable is not found throw an error
					throw new SyntaxException (WarningMessage.varYesDec (name));
				Lexeme l2 = lexemeList [i + 2]; //get the next lexeme
				if (l2.getName ().Equals (Constants.STARTINIT)) { //if it is ITZ
					Lexeme l3 = lexemeList [i + 3]; //get the lexeme after that
					int index = findVarName (l3.getName ()); //check if the variable is already declared
					if (l3.getDescription ().EndsWith ("constant")) { //check if it is a constant
						String[] type = l3.getDescription ().Split (spaces);
						table.Add (name, new Value (l3.getName (), type [0]));
						i += 3;
					} else if (l3.getName ().Equals ("\"")) { //check if it is a YARN constant
						Lexeme l4 = lexemeList [i + 4];
						String[] type = l4.getDescription ().Split (spaces);
						table.Add (name, new Value (l4.getName (), type [0]));
						i += 5;
					} else if ( index != -1) { //check if it is a declared variable
						table.Add (name, allTable[index][l3.getName ()]);
						i += 3;
					} else if (l3.getDescription ().Contains ("Operator")) { //check if it is an operator
						i += 3; //update the index
						string val = operatorList (lexemeList, ref i); //parse the operation
						string type = returnType(val); //get the most appropriate data type
						table.Add (name, new Value (val, type)); //add it to the table
					} else
						throw new SyntaxException (WarningMessage.expectedWord ("constant or variable", Constants.STARTINIT));
				} else
					throw new SyntaxException (WarningMessage.expectedWord (Constants.STARTINIT, "variable declaration"));
			} else
				throw new SyntaxException (WarningMessage.expectedWord ("variable declaration", Constants.STARTINIT));
		}
	}

	private void assignParse(ref int i){ //parses R
		//get the variable lexeme
		string varName = lexemeList [i-1].getName ();
		string varDesc = lexemeList [i-1].getDescription ();
		//get the value lexeme
		string valueName = lexemeList [i+1].getName ();
		string valueDesc = lexemeList [i+1].getDescription ();
		char[] spaces = { ' ' };
		int index = findVarName (varName);

		if (index == -1) //if the variable is not declared
			throw new SyntaxException (WarningMessage.varNoDec (varName));

		if (varDesc == Constants.VARDESC) { //if it is a variable
			if (valueDesc.EndsWith ("constant")) { //check if the value is constant
				String[] type = valueDesc.Split (spaces);
				Value old = table [varName];

				allTable[index][varName] = new Value (valueName, type [0]);
			} else if(valueName == "\""){ //check if the value is a YARN constant
				Lexeme yarn = lexemeList [i + 2];
				String[] type = yarn.getDescription ().Split (spaces);
				Value old = table [varName];

				allTable[index][varName] = new Value (yarn.getName (), type [0]);
				i+=2;
			}else if (valueDesc == Constants.VARDESC) { //check if the value is also a variable
				int index2 = findVarName (valueName);
				if (index2 == -1) //check if the variable is also declared
					throw new SyntaxException (WarningMessage.varNoDec (valueName));

				allTable[index][varName] = allTable[index2][valueName];
			} else if (valueDesc.Contains ("Operator")) { //check if it is an operator
				i++;
				string val = operatorList (lexemeList, ref i); //parse the expression first
				string type = returnType (val);
				i--;
				allTable[index][varName] = new Value (val, type);
			} else
				throw new SyntaxException (WarningMessage.RRightSide ());

		} else
			throw new SyntaxException (WarningMessage.RLefttSide ());

		i++;
	}

	private String operatorList(List<Lexeme> lexemeList, ref int index){ //check which operator is found
		if (lexemeList[index + 1].getDescription().Contains("break") 
			|| lexemeList[index+1].getDescription().Contains("comment")) //if an operator has no argument
			throw new SyntaxException (WarningMessage.noArguments(lexemeList[index].getName()));

		string name = lexemeList [index].getName (); //get the operator name
		string dec = lexemeList [index].getDescription (); //get the operator description
		string result = "";

		if (name.Equals (Constants.CONCAT)) { //if it is a SMOOSH
			index++;
			result = concatString (lexemeList, ref index); //parse it
		} else if(dec.Contains("Arity")) { //if it is ANY OF or ALL OF
			result = arityOperation(lexemeList, ref index); //parse it
		} else if (dec.Contains("Operator")){ //else
			result = mathOperation (lexemeList, ref index); //still parse it
		} else //only for saving grace. should not go here
			throw new WarningException(WarningMessage.unexpectedLexeme(name));

		index--;
		return result;
	}

	private String arityOperation(List<Lexeme> lexemeList, ref int index){ //converts the lexemes into stack
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";

		for (; !lexemeList [index].getDescription ().Contains ("break"); index++) { //gets all of the lexemes until the break
			String name = lexemeList [index].getName (); //get the lexeme name
			String desc = lexemeList [index].getDescription (); //get the lexeme description

			if (desc.Contains ("comment")) { //continue if it is a comment
				continue;
			} else if (name == Constants.AN) { //if an is found
				if (ANned) //if an is already found earlier
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else //else make the trigger true
					ANned = true;
			} else if (Constants.INTVAL.Match (name).Success ||
				Constants.FLOATVAL.Match (name).Success ||
				Constants.BOOLVAL.Match (name).Success) { //else check if it is an int, float, or TROOF
				if (ANned) {
					String[] type = desc.Split (delimiter); //get the type
					stackArity.Push (new Value (name, type [0])); //push it to stack
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.unexpectedLexeme (name));
			} else if (desc == Constants.VARDESC){ //if it is a variable
				int i = findVarName (name); //check if it is declared
				if (i != -1) {
					Value val = allTable[i][name]; //get the value
					stackArity.Push (val); //push it to stack
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.varNoDec (name));
			} else { //else push the operator to stack
				stackArity.Push (new Value (name, "Operator"));
				ANned = true;
			}
		}

		result = evaluateArity (); //evaluate the stack
		return result; //returns the result
	}

	private String evaluateArity(){ //evaluates the stack
		String result = "";
		Value popped;
		List<Value> values = new List<Value>(); //list of values in arity
		Stack<Value> temp = new Stack<Value> (); //temporary stack for operations
		bool start = true;

		while (stackArity.Count > 1) { //until the stack only has 1 element
			popped = stackArity.Pop(); //pop the stack

			if (popped.getValue () == Constants.MKAY) { //if it is an MKAY
				if (!start) //check if MKAY is already popped
					throw new SyntaxException (WarningMessage.unexpectedLexeme (popped.getValue ()));
				else { //else marks the start of the arity
					start = false;
					continue;
				}
			} else if (stackArity.Peek ().getValue () == Constants.NOT) { //else check if NOT is found
				stackArity.Pop (); //pop the NOT out
				//changes the value of popped and push it to stack
				values.Add(new Value((popped.getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.FALSE)); 
			} else if (popped.getType () != Constants.BOOL) { //if it is not a TROOF value
				if (popped.getType () != "Operator") { //check if it is not an operator (meaning INT or NUMBAR
					temp.Push (popped); //push it to temp stack
					while ((popped = stackArity.Pop ()).getType () != "Operator") {
						temp.Push (popped);
					}
					temp.Push (popped);
				} else { //else it is a boolean operator (BOTH OF, EITHER OF)
					//remove the recently pushed values
					temp.Push (values[values.Count - 1]);
					values.RemoveAt (values.Count - 1);

					temp.Push (values[values.Count - 1]);
					values.RemoveAt (values.Count - 1);

					temp.Push (popped);
				}
				//push the popped value in temporary stack to make it post fix
				while (temp.Count > 0) {
					stack.Push (temp.Pop ());
				}
				//evaluate the stack
				result = evaluateCond ();
				if (stackArity.Peek ().getValue () == Constants.NOT){ //check the next one if it is a NOT
					stackArity.Pop (); //pop the NOT out
					result = (result == Constants.TRUE) ? Constants.FALSE : Constants.TRUE; //changes the TROOF value
				}
				values.Add (new Value (result, Constants.BOOL)); //add the value to values
			} else { //else add the popped value to value
				values.Add (popped);
			}

			start = false;
		}
		//evaluate the arity
		popped = stackArity.Pop ();
		if (popped.getValue () == Constants.MANY_AND) { //for ALL OF
			foreach (Value v in values) {
				if (v.getValue () == Constants.FALSE) { //if false is found
					result = Constants.FALSE; //make the result false 
					break;
				}
			}
			if (result != Constants.FALSE)
				result = Constants.TRUE; //if no false is found, make it true
		} else if (popped.getValue () == Constants.MANY_OR) { //for ANY OF
			foreach (Value v in values) {
				if (v.getValue () == Constants.TRUE) { //if true is found
					result = Constants.TRUE;
				}
			}
			if (result != Constants.TRUE) //if no true is found make it false
				result = Constants.FALSE;
		} else
			throw new SyntaxException(WarningMessage.unexpectedLexeme(popped.getValue()));
		return result;
	}

	private String mathOperation(List<Lexeme> lexemeList, ref int index){ //pushing the math operation to stack
		bool ANned = true;
		char[] delimiter = {' '}; 
		string result = "";

		//get the lexeme name and description
		String name = lexemeList [index].getName ();
		String desc = lexemeList [index].getDescription ();

		//for each lexeme until the break or '!'
		for (; !desc.Contains("break") && name != Constants.NONEWLINE; index++,
		     name = lexemeList [index].getName (),
		     desc = lexemeList [index].getDescription ()) {
			if (name == Constants.A) { //if 'A' is found, stop pushing to stack
				break;
			} else if (desc.Contains ("comment")) { //continue of comment is found
				continue;
			} else if (name == Constants.AN) { //check if name is AN
				if (ANned) //if earlier AN is found
					throw new SyntaxException (WarningMessage.unexpectedLexeme (Constants.AN));
				else //else make the trigger true
					ANned = true;
			} else if (desc.Contains("constant")) { //if it is a constant
				if (ANned) {
					String[] type = desc.Split (delimiter);
					stack.Push (new Value (name, type [0])); //push the value to stack
					ANned = false;
				} else
					break;
			} else if (desc == Constants.VARDESC) { //else if it is a variable
				int i = findVarName (name);
				if (i != -1) {
					Value val = allTable [i][name]; //push the variable's value to stack
					stack.Push (val);
					ANned = false;
				} else
					throw new SyntaxException (WarningMessage.varNoDec (name));
			} else if (name == "\"") { //continue if double quote is found
				continue;
			} else { //if it is an operator push that operator to stack
				stack.Push (new Value (name, "Operator"));
				ANned = true;
			}
		}

		result = evaluateCond ();
		return result;
	}

	private String evaluateCond (){ //evaluate the stack
		List<Value> val = new List<Value> (); //list of values popped
		Value op = null;
		String result = "";


		while(stack.Count > 1){ //while the stack count is not one
			try{//pop the two values
				val.Add(stack.Pop()); //put it to a list of values
				if(stack.Count > 1){ //if there are more stack
					if(stack.Peek().getValue() == Constants.NOT){ //check if top of stack is NOT
						if(val[0].getType() != Constants.BOOL) //if it is NOT check if popped value is not a BOOL
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val[1].getValue()));
						else{
							stack.Pop(); //pop the NOT out
							val[0] = new Value((val[0].getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.BOOL); //switch the value of the latest added value
						}
					}
				}
				if(stack.Count > 1){ //if there are more to be popped
					val.Add(stack.Pop()); //add it again to the list
					if(stack.Peek().getValue() == Constants.NOT){ //check if NOT again is there
						if(val[1].getType() != Constants.BOOL) //check if the popped value is not a TROOF VALUE
							throw new SyntaxException(WarningMessage.unexpectedLexeme(val[1].getValue()));
						else{
							stack.Pop(); //pop the NOT out
							val[1] = new Value((val[1].getValue() == Constants.TRUE)? Constants.FALSE: Constants.TRUE, Constants.BOOL); //switch the TROOF value
						}
					}
				}
				op = stack.Pop(); //pop and check if it is an operator
				while(op.getType() != "Operator"){
					val.Add(op); //add the popped value to list of value until an operator is found
					op = stack.Pop();
				}
			} catch(Exception){
				throw new SyntaxException (WarningMessage.lackOperands() + " Resulted in stack underflow.");
			}

			//check the last two values added to the list
			int count = val.Count-1;
			Value val1 = val [count];
			Value val2 = val [count - 1];

			//get the values of the last two values added to the list
			String str1 = val1.getValue ();
			String str2 = val2.getValue ();
			String type1 = val1.getType ();
			String type2 = val2.getType ();
			String type = "";

			float f1 = float.MinValue;
			float f2 = float.MinValue;
			bool b1 = false;
			bool b2 = false;

			//initialize the values of the boolean values
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
			} else if(type1 != Constants.STRING || type2 != Constants.STRING) { //try parsing it if it is a string
				if (float.TryParse(str1, out f1)) {
					if (float.TryParse (str2, out f2)) {
						f1 = float.Parse (str1);
						f2 = float.Parse (str2);
						type = (val1.getType () == Constants.FLOAT || val2.getType () == Constants.FLOAT ||
							(val1.getType () == Constants.STRING && val2.getType () == Constants.STRING)) ? Constants.FLOAT : Constants.INT;
					}else
						throw new SyntaxException(WarningMessage.cannotConvert(str2, Constants.INT + " or " + Constants.FLOAT));
				} else
					throw new SyntaxException(WarningMessage.cannotConvert(str1, Constants.INT + " or " + Constants.FLOAT));
			}
				

			if (!op.getType ().Equals ("Operator")) //only for saving grace. should not go inside this if 
				throw new SyntaxException (WarningMessage.unexpectedLexeme (op.getValue ()));
			else {
				switch (op.getValue()) { //get what kind of operator is popped
				case Constants.ADD: //add two numbers
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 + f2); //if type is not a boolean add the two numbers
					else //else do not accept the operands
						throw new SyntaxException (WarningMessage.canAccept (Constants.ADD, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.SUB:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 - f2); //subtract the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.SUB, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MUL:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 * f2); //multiply the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MUL, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.DIV:
					if (type != Constants.BOOL) 
						result = Convert.ToString (f1 / f2); //divide the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.DIV, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MOD:
					if (type != Constants.BOOL)
						result = Convert.ToString (f1 % f2); //get the modulo of the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MOD, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MAX:
					if (type != Constants.BOOL)
						result = Convert.ToString ((f1 > f2) ? f1 : f2); //get the bigger of the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MAX, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.MIN:
					if (type != Constants.BOOL)
						result = Convert.ToString ((f1 < f2) ? f1 : f2); //get the smaller of the two numbers
					else
						throw new SyntaxException (WarningMessage.canAccept (Constants.MIN, Constants.INT + " OR " + Constants.FLOAT));
					break;
				case Constants.EQUAL:
					if (type1 == type2 && type1 != Constants.NOTYPE) { //check if the types are the same
						if (type1 != "TROOF")
							result = (f1 == f2) ? Constants.TRUE : Constants.FALSE; //if same and are integer/float, check if equal
						else
							result = (str1 == str2) ? Constants.TRUE : Constants.FALSE; //else check if the strings are equal
					}else //if not return FAIL
						result = Constants.FALSE;
					type = Constants.BOOL;
					break;
				case Constants.NOTEQUAL:
					if (type1 == type2 && type1 != Constants.NOTYPE) {
						if (type1 != "TROOF")
							result = (f1 != f2) ? Constants.TRUE : Constants.FALSE; //checks if the numbers are not equal
						else
							result = (str1 != str2) ? Constants.TRUE : Constants.FALSE; //check if the strings are not equal
					}else //returns WIN if types are not equal
						result = Constants.TRUE;
					type = Constants.BOOL;
					break;
				case Constants.AND:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.AND, Constants.BOOL));
					else result = (b1 && b2) ? Constants.TRUE : Constants.FALSE; //check if both boolean values are true
					break;
				case Constants.OR:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.OR, Constants.BOOL));
					else result = (b1 || b2) ? Constants.TRUE : Constants.FALSE; //check if at least one boolean value is true
					break;
				case Constants.XOR:
					if (type1 != Constants.BOOL || type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.XOR, Constants.BOOL));
					else result = (b1 ^ b2) ? Constants.TRUE : Constants.FALSE; //check if at least one boolean value is true and the other is not true
					break;
				case Constants.NOT:
					if (type2 != Constants.BOOL)
						throw new SyntaxException (WarningMessage.canAccept(Constants.NOT, Constants.BOOL));
					else result = (!b2) ? Constants.TRUE : Constants.FALSE; //changes the TROOF value if NOT is found
					break;
				default: //for saving grace. should not go in this default case.
					throw new Exception ("Something went wrong in evaluate");
				}

				try{
					if (stack.Peek ().getValue () == Constants.NOT) { //check if top of stack is NOT
						if (type != Constants.BOOL)
							throw new SyntaxException (WarningMessage.unexpectedLexeme (result));
						stack.Pop(); //pop the NOT out
						result = (result != Constants.TRUE) ? Constants.TRUE : Constants.FALSE; //changes the troof value
					}
				} 
				catch(Exception  e){
					if(e.GetType().Name == "SyntaxException") //throw the syntax exception if found
						throw e;
				}
				
				stack.Push (new Value(result, type)); //push the result back to stack
				//remove the last two popped values
				val.RemoveAt (val.Count - 1); 
				val.RemoveAt (val.Count - 1);
				//pop the popped values in values list back to stack
				while(val.Count != 0) {
					stack.Push (val[val.Count-1]);
					val.RemoveAt (val.Count - 1);
				}
			}
		}

		result = stack.Pop ().getValue (); //get the result
		return result; //return the result
	}

	private String concatString(List<Lexeme> lexemeList, ref int index){ //for concatenating strings
		String result = "";
		bool hasEnded = false;
		bool needNext = false;

		//gets the list of lexemes until the break
		for (; !lexemeList[index].getDescription().Contains("break"); index++) {
			string name = lexemeList [index].getName ();
			string dec = lexemeList [index].getDescription ();

			if (dec.Contains ("comment")) { //if comment is found, continue
				continue;
			} else if (hasEnded) //if MKAY is already found
				throw new SyntaxException (WarningMessage.unexpectedLexeme(name));
			else if (name.Equals(Constants.AN)){ //if AN is found
				if (needNext) //if AN is not yet found before this AN
					needNext = false;
				else
					throw new SyntaxException (WarningMessage.unexpectedLexeme(Constants.AN));
			} else if (dec.EndsWith ("constant")) { //if it is constant
				result += name; //concatenate it to result
				needNext = true;
			}
			else if (dec.EndsWith (Constants.VARDESC)) { //if it is a varable
				int i = findVarName (name); //check if the variable exists in the 
				if (i != -1) {
					string varname = name;
					if (allTable [i] [varname].getValue ().Equals (Constants.NULL)) //if NOOB is found
						result += "null"; //concatenate null
					else
						result += allTable[i][varname].getValue (); //concatenate the value of the variable
				} else
					throw new SyntaxException (WarningMessage.varNoDec(name));
				needNext = true;
			} else if (name.Equals (Constants.MKAY)) { //if MKAY is found
				hasEnded = true; //mark the end of the statement
			} else if(name.Equals("\"")){
				continue;
			} else
				throw new SyntaxException(WarningMessage.unexpectedLexeme(name));
		}
		return result; //return the result
	}

	public void UpdateLexemes(List<Lexeme> list){ //update the list of lexemes in lexemes tree view
		foreach (Lexeme l in list) {
			tokensListStore.AppendValues (l.getName(), l.getDescription());
		}
	}

	public void UpdateTables(){ //update the symbol table tree view and changes \t to :>
		symbolTableListStore.Clear (); //clears the list store
		foreach(Dictionary<string, Value> t in allTable){ //for each dictionary
			foreach (string key in t.Keys) { //for each keys
				Value val = t [key];
				if (val.getValue ().Contains ("\t")) //if the value contains a \t
					val = t [key] = new Value (val.getValue ().Replace ("\t", ":>"), val.getType ()); //change it to :>
				symbolTableListStore.AppendValues (key, val.getValue (), val.getType ()); //add it to symbol table tree view
			}
		}
	}

	public String returnType(String val){ //checks for the most appropriate return type
		String type = "";

		if (Constants.INTVAL.Match (val).Success) //if value matches int
			type = Constants.INT; //make the type NUMBR
		else if (Constants.FLOATVAL.Match (val).Success) //if the value matches float
			type = Constants.FLOAT; //make the type NUMBAR
		else if (Constants.BOOLVAL.Match (val).Success) //if the value matches bool
			type = Constants.BOOL; //make the type TROOF
		else //else make the type YARN
			type = Constants.STRING;

		return type; //return the type
	}

	private int findVarName(string varname){ //finds the variable in the list of dictionaries in symbol table
		for (int i = allTable.Count-1; i >= 0; i--) { //from the last symbol table to the 0th symbol table
			if (allTable [i].ContainsKey (varname)) //if it contains the variable name
				return i; //return the index
		}

		return -1; //else return -1
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a){ //when close button is clicked
		Gtk.Application.Quit();
		a.RetVal = true;
	}
	
	protected void runProgramClick (object sender, EventArgs e){ //when run program is clicked
		Interpret ();
	}

	private void openFile (object sender, EventArgs e){ //when opening a file in file menu is clicked
		//open the file chooser
		Gtk.FileChooserDialog filechooser =
			new Gtk.FileChooserDialog("Choose the file to open",
				this,
				FileChooserAction.Open,
				"Cancel",ResponseType.Cancel,
				"Open",ResponseType.Accept);

		//filter all files to only see a lolcode source code
		filechooser.Filter = new FileFilter ();
		filechooser.Filter.Name = "LOLCode";
		filechooser.Filter.AddPattern("*.lol");
		filechooser.SetCurrentFolder ("./../../../../");
		filechooser.SelectMultiple = false;

		//if open is clicked
		if (filechooser.Run() == (int)ResponseType.Accept) 
		{
			sourceText.Buffer.Text = "";

			this.pageLabel.Text = filechooser.Filename;
			System.IO.FileStream file = System.IO.File.OpenRead(filechooser.Filename);
			try{
				using(StreamReader reader = new StreamReader(file)){
					string line = reader.ReadToEnd();
					sourceText.Buffer.Text = line; //get the content and put it to sourceText
					reader.Close();
				}
			}catch(Exception ex){ 
				this.outputField.Buffer.Text += "\nThe file could not be read: "+ ex.Message;
			}
			file.Close();
		}
		filechooser.Destroy(); //close the filechoooser
	}
	
	protected void CloseOnClick (object sender, EventArgs e) //when exit in file menu is clicked
	{
		Environment.Exit (0);
	}

	protected void runProgramButton (object o, KeyPressEventArgs args) //when F5 is pressed
	{
		if (args.Event.Key == Gdk.Key.F5) { //Interpret when F5 is pressedd
			Interpret ();
		}
	}
	protected void aboutUs (object sender, EventArgs e) //when about is is pressed
	{
		AboutDialog about = new AboutDialog ();
		about.ProgramName = "ANG GANDA NI MAAM KAT LOLTERPRETER KEK";
		about.Version = "1.0";
		about.Copyright = "(c) OutlawTechnoPsychobitches : Julius Jireh B. Vega, Aron John S. Vibar, Maru Gabriel S. Baul";
		about.Comments = @"LOLCode Interpreter for creating, editing, and executing LOLCode v1.2 programs";
		try{
			about.Logo = new Gdk.Pixbuf("../../logo.png");
		}
		catch(Exception ){}
		about.Run();
		about.Destroy();
	}

	protected void saveAs (object sender, EventArgs e) //when save as is clicked
	{
		//open filechooser
		Gtk.FileChooserDialog fileChooser = new Gtk.FileChooserDialog (
			"Save LOLCode",
			this,
			FileChooserAction.Save,
			"Cancel", ResponseType.Cancel,
			"Save", ResponseType.Accept);

		fileChooser.SetCurrentFolder ("./../../../../");
		fileChooser.SelectMultiple = false;
		//filter so that lolcode sourcecode is found
		fileChooser.Filter = new FileFilter ();
		fileChooser.Filter.Name = "LOLCode";
		fileChooser.Filter.AddPattern ("*.lol");


		if (fileChooser.Run () == (int)ResponseType.Accept) {
			{
				//delete the file 
				try{
					File.Delete (fileChooser.Filename);
				}catch(Exception ){}
				System.IO.FileStream filestream;
				if (fileChooser.Filename.EndsWith (".lol")) { 
					//open the file name if filename already ends in .lol
					filestream = System.IO.File.OpenWrite (fileChooser.Filename);
					this.pageLabel.Text = fileChooser.Filename; //set the page label to filename
				} 
				else { 
					//open the filename with .lol appended with it
					filestream = System.IO.File.OpenWrite (fileChooser.Filename + ".lol");
					this.pageLabel.Text = fileChooser.Filename+".lol"; //set the page label to filename
				}
				try{
					//write the contents to file
					using(StreamWriter writer = new StreamWriter(filestream)){

						String text = sourceText.Buffer.Text;
						writer.WriteLine(text);
						writer.Close();
					}
				}catch(Exception ex){
					this.outputField.Buffer.Text += "\nThe file could not be saved: "+ ex.Message;
				}
				filestream.Close(); //close the filestream
			}
		}
		fileChooser.Destroy (); //close the filechooser
	}

	protected void save (object sender, EventArgs e) //when save in file menu is clicked
	{
		String filename = this.pageLabel.Text; //get the filename
		if (!filename.StartsWith("/")) { //if file is not yet saved
			this.saveAs (sender, e); //save the file with new filename
		}
		else {
			//delete the file
			try {
				File.Delete (filename);
			} catch (Exception ) {
			}

			System.IO.FileStream filestream = System.IO.File.OpenWrite (filename);
			try {
				//write the contents to file
				using (StreamWriter writer = new StreamWriter(filestream)) {

					String text = sourceText.Buffer.Text;
					writer.WriteLine (text);
					writer.Close ();
				}
			} catch (Exception ex) {
				this.outputField.Buffer.Text += "\nThe file could not be saved: " + ex.Message;
			}
			//close the filestream
			filestream.Close ();
		}
	}
}