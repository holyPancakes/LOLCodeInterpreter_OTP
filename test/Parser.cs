using System;
using System.Collections.Generic;

namespace test
{
	public class Parser
	{
		private Dictionary<String, Value> table; //symbol table
		private Boolean hasEnded; //checks if the program already ended using KTHXBYE

		public Parser(){ //constructor
			table = new Dictionary<String, Value>();
			hasEnded = false;
		}

		public void processHAI(List<Lexeme> lex)
		{ //checks if the program starts with HAI
			Lexeme top = lex.Remove(0);
			if(!top.getName().Equals("HAI"))
				throw new Exception("All programs should start with HAI!");
		}

		public void process(List<Lexeme> lex, Boolean file)
		{ //processses the lexemes
			//Console.log.println("PARSING:");
			for(int i=0; i < lex.Length; i++){
				if(hasEnded)
				{
					throw new Exception("Program already ended!");
				}
				else
				{
					if(lex.get(i).getName().Equal("VISIBLE")){ //checks if the keyword is VISIBLE
						if(lex.size() - i >= 2){ //checks if VISIBLE has arguments
							Lexeme l = lex.get(i+1); //gets the next lexeme
							if(l.getDescription().endsWith("YARN constant")){ //checks if it is a string constant
								Console.log.println(l.getName()); //prints the string
							}else if(l.getDescription().Equal("Variable Identifier")){
								if(!table.containsKey(l.getName())) //checks if the variable is already declared
									throw new Exception("Variable identifier '" + l.getName() + "' is not yet declared.");
								Value val = table.get(l.getName()); //gets the value of the variable
								Console.log.println(val.getValue()); //prints the value of the variable
							}else throw new Exception(l.getName() + " is not printable!"); //error that a value is not printable
							i++; //increments the index
						}else //else VISIBLE has no arguments
							throw new Exception("VISIBLE has no arguments!");
					}else if(lex.get(i).getName().Equal("I HAS A")){ //checks if the keyword is I HAS A
						if(lex.size() - i == 2){ //checks if I HAS A has arguments
							Lexeme l = lex.get(i+1); //gets the next lexeme
							if(l.getDescription().Equal("Variable Identifier")){ //checks if it is a variable identifier
								if(table.containsKey(l.getName())) //checks if the variable is already in the table
									throw new Exception("Variable identifier " + l.getName() + " already exists.");
								table.put(l.getName(), new Value("NOOB", "Untyped")); //makes the variable NOOB
								if(!file) Console.log.println("Initialized " + l.getName() + " to NOOB.");
							}
							i++; //increments the index
						}
						else if(lex.size() > 2){ //checks if the statement also starts with a value
							Lexeme l1 = lex.get(i+1); //gets the next lexeme
							if(l1.getDescription().Equal("Variable Identifier")){ //checks if it is a variable identifier
								if(table.containsKey(l1.getName())) //checks if the variable is in the table
									throw new Exception("Variable " + l1.getName() + " already exists.");
								Lexeme l2 = lex.get(i+2); //gets the next lexeme of next lexeme
								if(l2.getDescription().Equal("Assigns value after declaration.")){ //checks if it is ITZ
									Lexeme l3 = lex.get(i+3); //gets the argument of ITZ
									if(l3.getDescription().endsWith("constant")){ //checks if the argument is contstant
										String[] type = l3.getDescription().Split(" "); //gets the datatype of the value
										table.put(l1.getName(), new Value(l3.getName(), type[0])); //puts it to table
										if(!file) Console.log.println("Initialized " + l1.getName() + " to " + l3.getName() + ".");
									}else if(table.containsKey(l3.getName())){ //checks if the argument is a variable and it is initialized
										table.put(l1.getName(), table.get(l3.getName())); //copies the value and puts it to the table
										if(!file) Console.log.println("Initialized " + l1.getName() + " to " + table.get(l3.getName()).getValue() + ".");
									}else //else ITZ has no arguments
										throw new Exception("Expected constant or variable after ITZ.");
								}
							}
							i+=3;
						}
						else{ //else throws exception
							throw new Exception("I HAS A has no arguments!");
						}
					}else if(lex.get(i).getName().Equals("R")){ //checks if the keyword is R
						Lexeme var = lex.get(i-1); //gets the left and right lexemes of R
						Lexeme value = lex.get(i+1);
						if(var.getDescription().Equal("Variable Identifier")){ //checks if the left side is a variable
							if(value.getDescription().endsWith("constant")){ //checks if the right side is a constant
								String[] type = value.getDescription().Split(" "); //gets the dataype
								Value old = table.get(var.getName()); //gets the old datatype of the variable

								if(!old.getType().Equal(type[0])) //check if they are differen
									if(!file) Console.log.println("Changed " + var.getName() + " type from " + old.getType() + " to " +type[0]);

								table[var.getName()] = new Value(value.getName(), type[0]); //puts new value to table
								if(!file) Console.log.println("Changed " + var.getName() + " to " + value.getName() + ".");
							}else if(value.getDescription().Equal("Variable Identifier")){ //checks if the right side is a variable
								if(!table.containsKey(value.getName())) //check if the variable is already declared
									throw new Exception("Variable " + value.getName() + " is not yet declared!");

								String type = table.get(value.getName()).getType(); //gets the datatype of the variable
								Value old = table.get(var.getName()); //gets the old value of the variable

								if(!old.getType().Equal(type)) //checks if the dataypes are different
									if(!file) 
										Console.log.println("Changed " + var.getName() + " type from " + old.getType() + " to " +type);

								table.[var.getName()] = table.[value.getName()]; //changes the value of the variable
								if(!file) Console.log.println("Initialized " + var.getName() + " to " + table.get(value.getName()).getValue() + ".");
							}else //else the right side is neither a variable or a constant
								throw new Exception("Only variable or constants should be on left hand side of R");

						}else //else the left side is not a vairblae
							throw new Exception("Variable should be on left hand side of R. " + var.getName() + " is not a variable.");

						i++;
					}else if(lex.get(i).getName().Equal("KTHXBYE")){ //checks if the keyword is KTHXBYE
						hasEnded = true;
					}else if(lex.get(i).getDescription().Equal("Variable Identifier") && !file){ //checks if the line typed is a variable
						if(!table.containsKey(lex.get(i).getName()))
							throw new Exception("Variable identifier '" + lex.get(i).getName() + "' is not yet declared.");
						else if(i == lex.size()-1){
							Value val = table.get(lex.get(i).getName());
							if(!file) Console.log.println(lex.get(i).getName() + " currently contains " + val.getValue() + " with type " + val.getType());
						}
					}
				}
			}
			//Console.log.println("========");
		}
	}
}

