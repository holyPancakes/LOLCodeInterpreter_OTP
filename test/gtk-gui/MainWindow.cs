
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	
	private global::Gtk.Action MenuAction;
	
	private global::Gtk.Action yeayAction;
	
	private global::Gtk.Action MenuAction1;
	
	private global::Gtk.Action YeayAction;
	
	private global::Gtk.Action ViewAction;
	
	private global::Gtk.Action HelpAction;
	
	private global::Gtk.Action NotForYouAction;
	
	private global::Gtk.Action FileAction;
	
	private global::Gtk.Action EditAction;
	
	private global::Gtk.Action HelpAction1;
	
	private global::Gtk.Action ViewAction1;
	
	private global::Gtk.Action EditAction1;
	
	private global::Gtk.Action HelpAction2;
	
	private global::Gtk.Action OpenFileAction;
	
	private global::Gtk.Action SaveAction;
	
	private global::Gtk.Action SaveAsAction;
	
	private global::Gtk.Action ExitAction;
	
	private global::Gtk.Action AboutAction;
	
	private global::Gtk.Action AboutAction1;
	
	private global::Gtk.Action RunAction;
	
	private global::Gtk.Action RunProgramAction;
	
	private global::Gtk.Action RunProgramF5Action;
	
	private global::Gtk.VBox mainBox;
	
	private global::Gtk.MenuBar menubar;
	
	private global::Gtk.HBox contents;
	
	private global::Gtk.VBox sourceAndConsole;
	
	private global::Gtk.Notebook sourcePages;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	
	private global::Gtk.TextView sourceText;
	
	private global::Gtk.Label pageLabel;
	
	private global::Gtk.VBox console;
	
	private global::Gtk.Label consoleLabel;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow3;
	
	private global::Gtk.TextView outputField;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow4;
	
	private global::Gtk.TextView lineField;
	
	private global::Gtk.VBox tokensAndSymbolTable;
	
	private global::Gtk.Label tokensLabel;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow2;
	
	private global::Gtk.TreeView symbolTableTreeView;
	
	private global::Gtk.Label symbolTableLabel;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	
	private global::Gtk.TreeView tokensTreeView;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.MenuAction = new global::Gtk.Action ("MenuAction", global::Mono.Unix.Catalog.GetString ("Menu"), null, null);
		this.MenuAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Menu");
		w1.Add (this.MenuAction, null);
		this.yeayAction = new global::Gtk.Action ("yeayAction", global::Mono.Unix.Catalog.GetString ("yeay"), null, null);
		this.yeayAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("yeay");
		w1.Add (this.yeayAction, null);
		this.MenuAction1 = new global::Gtk.Action ("MenuAction1", global::Mono.Unix.Catalog.GetString ("Menu"), null, null);
		this.MenuAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Menu");
		w1.Add (this.MenuAction1, null);
		this.YeayAction = new global::Gtk.Action ("YeayAction", global::Mono.Unix.Catalog.GetString ("Yeay"), null, null);
		this.YeayAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Yeay");
		w1.Add (this.YeayAction, null);
		this.ViewAction = new global::Gtk.Action ("ViewAction", global::Mono.Unix.Catalog.GetString ("View"), null, null);
		this.ViewAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("View");
		w1.Add (this.ViewAction, null);
		this.HelpAction = new global::Gtk.Action ("HelpAction", global::Mono.Unix.Catalog.GetString ("Help"), null, null);
		this.HelpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
		w1.Add (this.HelpAction, null);
		this.NotForYouAction = new global::Gtk.Action ("NotForYouAction", global::Mono.Unix.Catalog.GetString ("Not For You"), null, null);
		this.NotForYouAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Not For You");
		w1.Add (this.NotForYouAction, null);
		this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("File"), null, null);
		this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Menu");
		w1.Add (this.FileAction, null);
		this.EditAction = new global::Gtk.Action ("EditAction", global::Mono.Unix.Catalog.GetString ("Edit"), null, null);
		this.EditAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Edit");
		w1.Add (this.EditAction, null);
		this.HelpAction1 = new global::Gtk.Action ("HelpAction1", global::Mono.Unix.Catalog.GetString ("Help"), null, null);
		this.HelpAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
		w1.Add (this.HelpAction1, null);
		this.ViewAction1 = new global::Gtk.Action ("ViewAction1", global::Mono.Unix.Catalog.GetString ("View"), null, null);
		this.ViewAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("View");
		w1.Add (this.ViewAction1, null);
		this.EditAction1 = new global::Gtk.Action ("EditAction1", global::Mono.Unix.Catalog.GetString ("Edit"), null, null);
		this.EditAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Edit");
		w1.Add (this.EditAction1, null);
		this.HelpAction2 = new global::Gtk.Action ("HelpAction2", global::Mono.Unix.Catalog.GetString ("Help"), null, null);
		this.HelpAction2.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
		w1.Add (this.HelpAction2, null);
		this.OpenFileAction = new global::Gtk.Action ("OpenFileAction", global::Mono.Unix.Catalog.GetString ("Open File"), null, null);
		this.OpenFileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save");
		w1.Add (this.OpenFileAction, null);
		this.SaveAction = new global::Gtk.Action ("SaveAction", global::Mono.Unix.Catalog.GetString ("Save"), null, null);
		this.SaveAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save as...");
		w1.Add (this.SaveAction, null);
		this.SaveAsAction = new global::Gtk.Action ("SaveAsAction", global::Mono.Unix.Catalog.GetString ("Save as..."), null, null);
		this.SaveAsAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Exit");
		w1.Add (this.SaveAsAction, null);
		this.ExitAction = new global::Gtk.Action ("ExitAction", global::Mono.Unix.Catalog.GetString ("Exit"), null, null);
		this.ExitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Exit");
		w1.Add (this.ExitAction, null);
		this.AboutAction = new global::Gtk.Action ("AboutAction", global::Mono.Unix.Catalog.GetString ("About"), null, null);
		this.AboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("About");
		w1.Add (this.AboutAction, null);
		this.AboutAction1 = new global::Gtk.Action ("AboutAction1", global::Mono.Unix.Catalog.GetString ("About"), null, null);
		this.AboutAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("About");
		w1.Add (this.AboutAction1, null);
		this.RunAction = new global::Gtk.Action ("RunAction", global::Mono.Unix.Catalog.GetString ("Run"), null, null);
		this.RunAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Run");
		w1.Add (this.RunAction, null);
		this.RunProgramAction = new global::Gtk.Action ("RunProgramAction", global::Mono.Unix.Catalog.GetString ("Run program"), null, null);
		this.RunProgramAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Run program");
		w1.Add (this.RunProgramAction, null);
		this.RunProgramF5Action = new global::Gtk.Action ("RunProgramF5Action", global::Mono.Unix.Catalog.GetString ("Run Program (F5)"), null, null);
		this.RunProgramF5Action.ShortLabel = global::Mono.Unix.Catalog.GetString ("Run Program");
		w1.Add (this.RunProgramF5Action, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.WidthRequest = 800;
		this.HeightRequest = 600;
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("ANG GANDA NI MAAM KAT LOLCODE INTERPRETER");
		this.WindowPosition = ((global::Gtk.WindowPosition)(3));
		this.Resizable = false;
		this.AllowGrow = false;
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.mainBox = new global::Gtk.VBox ();
		this.mainBox.Name = "mainBox";
		this.mainBox.Spacing = 6;
		// Container child mainBox.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar'><menu name='FileAction' action='FileAction'><menuitem name='OpenFileAction' action='OpenFileAction'/><menuitem name='SaveAction' action='SaveAction'/><menuitem name='SaveAsAction' action='SaveAsAction'/><menuitem name='ExitAction' action='ExitAction'/></menu><menu name='ViewAction1' action='ViewAction1'/><menu name='EditAction1' action='EditAction1'/><menu name='RunAction' action='RunAction'><menuitem name='RunProgramF5Action' action='RunProgramF5Action'/></menu><menu name='HelpAction2' action='HelpAction2'><menuitem name='AboutAction1' action='AboutAction1'/></menu></menubar></ui>");
		this.menubar = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar")));
		this.menubar.Name = "menubar";
		this.mainBox.Add (this.menubar);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.mainBox [this.menubar]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child mainBox.Gtk.Box+BoxChild
		this.contents = new global::Gtk.HBox ();
		this.contents.Name = "contents";
		this.contents.Spacing = 6;
		// Container child contents.Gtk.Box+BoxChild
		this.sourceAndConsole = new global::Gtk.VBox ();
		this.sourceAndConsole.Name = "sourceAndConsole";
		this.sourceAndConsole.Spacing = 6;
		// Container child sourceAndConsole.Gtk.Box+BoxChild
		this.sourcePages = new global::Gtk.Notebook ();
		this.sourcePages.WidthRequest = 500;
		this.sourcePages.HeightRequest = 200;
		this.sourcePages.CanFocus = true;
		this.sourcePages.Name = "sourcePages";
		this.sourcePages.CurrentPage = 0;
		// Container child sourcePages.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.sourceText = new global::Gtk.TextView ();
		this.sourceText.CanFocus = true;
		this.sourceText.Name = "sourceText";
		this.GtkScrolledWindow.Add (this.sourceText);
		this.sourcePages.Add (this.GtkScrolledWindow);
		// Notebook tab
		this.pageLabel = new global::Gtk.Label ();
		this.pageLabel.Name = "pageLabel";
		this.pageLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("source.lol");
		this.sourcePages.SetTabLabel (this.GtkScrolledWindow, this.pageLabel);
		this.pageLabel.ShowAll ();
		this.sourceAndConsole.Add (this.sourcePages);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.sourceAndConsole [this.sourcePages]));
		w5.Position = 0;
		// Container child sourceAndConsole.Gtk.Box+BoxChild
		this.console = new global::Gtk.VBox ();
		this.console.WidthRequest = 500;
		this.console.Name = "console";
		this.console.Spacing = 6;
		// Container child console.Gtk.Box+BoxChild
		this.consoleLabel = new global::Gtk.Label ();
		this.consoleLabel.Name = "consoleLabel";
		this.consoleLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Console");
		this.console.Add (this.consoleLabel);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.console [this.consoleLabel]));
		w6.Position = 0;
		w6.Expand = false;
		w6.Fill = false;
		// Container child console.Gtk.Box+BoxChild
		this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow3.HeightRequest = 116;
		this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
		this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
		this.outputField = new global::Gtk.TextView ();
		this.outputField.Name = "outputField";
		this.outputField.Editable = false;
		this.outputField.CursorVisible = false;
		this.GtkScrolledWindow3.Add (this.outputField);
		this.console.Add (this.GtkScrolledWindow3);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.console [this.GtkScrolledWindow3]));
		w8.Position = 1;
		// Container child console.Gtk.Box+BoxChild
		this.GtkScrolledWindow4 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow4.HeightRequest = 30;
		this.GtkScrolledWindow4.Name = "GtkScrolledWindow4";
		this.GtkScrolledWindow4.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow4.Gtk.Container+ContainerChild
		this.lineField = new global::Gtk.TextView ();
		this.lineField.CanFocus = true;
		this.lineField.Name = "lineField";
		this.lineField.Editable = false;
		this.GtkScrolledWindow4.Add (this.lineField);
		this.console.Add (this.GtkScrolledWindow4);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.console [this.GtkScrolledWindow4]));
		w10.Position = 2;
		w10.Expand = false;
		this.sourceAndConsole.Add (this.console);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.sourceAndConsole [this.console]));
		w11.Position = 1;
		this.contents.Add (this.sourceAndConsole);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.contents [this.sourceAndConsole]));
		w12.Position = 0;
		w12.Expand = false;
		w12.Fill = false;
		// Container child contents.Gtk.Box+BoxChild
		this.tokensAndSymbolTable = new global::Gtk.VBox ();
		this.tokensAndSymbolTable.Name = "tokensAndSymbolTable";
		this.tokensAndSymbolTable.Spacing = 6;
		// Container child tokensAndSymbolTable.Gtk.Box+BoxChild
		this.tokensLabel = new global::Gtk.Label ();
		this.tokensLabel.Name = "tokensLabel";
		this.tokensLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Tokens");
		this.tokensAndSymbolTable.Add (this.tokensLabel);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.tokensAndSymbolTable [this.tokensLabel]));
		w13.Position = 0;
		w13.Expand = false;
		w13.Fill = false;
		// Container child tokensAndSymbolTable.Gtk.Box+BoxChild
		this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow2.WidthRequest = 300;
		this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
		this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
		this.symbolTableTreeView = new global::Gtk.TreeView ();
		this.symbolTableTreeView.CanFocus = true;
		this.symbolTableTreeView.Name = "symbolTableTreeView";
		this.GtkScrolledWindow2.Add (this.symbolTableTreeView);
		this.tokensAndSymbolTable.Add (this.GtkScrolledWindow2);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.tokensAndSymbolTable [this.GtkScrolledWindow2]));
		w15.PackType = ((global::Gtk.PackType)(1));
		w15.Position = 1;
		// Container child tokensAndSymbolTable.Gtk.Box+BoxChild
		this.symbolTableLabel = new global::Gtk.Label ();
		this.symbolTableLabel.Name = "symbolTableLabel";
		this.symbolTableLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Symbol Table");
		this.tokensAndSymbolTable.Add (this.symbolTableLabel);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.tokensAndSymbolTable [this.symbolTableLabel]));
		w16.PackType = ((global::Gtk.PackType)(1));
		w16.Position = 2;
		w16.Expand = false;
		w16.Fill = false;
		// Container child tokensAndSymbolTable.Gtk.Box+BoxChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.WidthRequest = 300;
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.tokensTreeView = new global::Gtk.TreeView ();
		this.tokensTreeView.CanFocus = true;
		this.tokensTreeView.Name = "tokensTreeView";
		this.GtkScrolledWindow1.Add (this.tokensTreeView);
		this.tokensAndSymbolTable.Add (this.GtkScrolledWindow1);
		global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.tokensAndSymbolTable [this.GtkScrolledWindow1]));
		w18.PackType = ((global::Gtk.PackType)(1));
		w18.Position = 3;
		this.contents.Add (this.tokensAndSymbolTable);
		global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.contents [this.tokensAndSymbolTable]));
		w19.Position = 1;
		w19.Expand = false;
		w19.Fill = false;
		this.mainBox.Add (this.contents);
		global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.mainBox [this.contents]));
		w20.Position = 1;
		this.Add (this.mainBox);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 819;
		this.DefaultHeight = 633;
		this.Show ();
		this.OpenFileAction.Activated += new global::System.EventHandler (this.openFile);
		this.ExitAction.Activated += new global::System.EventHandler (this.CloseOnClick);
		this.RunProgramF5Action.Activated += new global::System.EventHandler (this.runProgramClick);
	}
}
