using System;
using Gtk;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init (); //initializes the application
			MainWindow win = new MainWindow (); //creates the window
			win.Show (); //shows the window
			Application.Run (); //runs the application
		}
	}
}
