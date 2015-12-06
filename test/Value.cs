using System;

/* Authors:
 * Baul, Maru Gabriel S.
 * Vega, Julius Jireh B.
 * Vibar, Aron John S.
 */
namespace test
{
	//holds the actual value and the corresponding datatype
	public class Value
	{
		private String value;
		private String type;

		public Value(String v, String t){
			this.value = v;
			this.type = t;
		}

		public String getValue(){
			return this.value;
		}

		public String getType(){
			return this.type;
		}
	}
}

