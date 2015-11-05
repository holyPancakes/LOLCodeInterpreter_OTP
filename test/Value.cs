using System;

namespace test
{
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

