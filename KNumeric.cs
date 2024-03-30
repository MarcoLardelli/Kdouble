/*
                                
 
 (c) Marco Lardelli, Zuerich 2012
 
 MIT Licence

*/

using System.Collections;
using System.Collections.Generic;

public class Kdouble {
	
	/// <summary>
	/// Float numbers get rounded to this no of digits
	/// </summary>
	public static int Digits=3;
	
	private string expression="";
	public string Expression {
		get {return expression;}
	}
	
	private double sampleValue;
	public double SampleValue {
		get {return sampleValue;}
	}
	
	private int cc; // computational cost of expression
	public int Cc {
		get {return cc;}
	}
	
	private bool isNumber=false;
	public bool IsNumber {  // this is true, if the expression corresponds to a simple number (doubleVar.ToString())
		get {return isNumber; }
		private set {isNumber=value;}
	}	
	
	private bool isVariable=false;  // input variables should not be translated/simplified to variables 
	public bool IsVariable {
		get {return isVariable; }
		private set {isVariable=value;}
	}
	
	private static Dictionary<string,int> variables = new Dictionary<string, int>();
	private static int variablesCounter=0;
	
	// the constructors
	
	// special factory for input variables. makes sure cc is 1 and isVariable=true
	public static Kdouble Input(string e,double sValue=1f) {
		return new Kdouble(e,sValue,1,true);
	}
	
	public Kdouble(string e,double sValue=1f,int cc=1,bool isVariable=false) {  // cc==1 corresponds to a simple expression consisting of one variable (i.e. cost to fetch data from variable)
		// whenever a new Kdouble is created from an expression, check if a variable corresponding to the expression already exists
		if (isVariable)
			expression=e;
		else
			expression=Simplify(e);
		sampleValue=sValue;
		IsNumber=false;
		IsVariable=isVariable;
		this.cc=cc;  // i.e. fetch value from variable
	}
	
	public Kdouble(double value) {
		expression=value.ToString();
		sampleValue=value;
		IsNumber=true;
		IsVariable=false;
		cc=0;
	}
	
	public Kdouble(Kdouble kf) {
		// just copy all values
		expression=kf.expression;
		sampleValue=kf.sampleValue;
		IsNumber=kf.isNumber;
		IsVariable=kf.isVariable;
		cc=kf.cc;
	}
	
	
	// overloaded operators
	
	public static Kdouble operator + (Kdouble a, Kdouble b) {
		// both are numbers -> reduce to one number with added value
		if (a.isNumber && b.isNumber) {
			return new Kdouble(a.sampleValue+b.sampleValue);
		}
		
		a=Simplify(a);
		b=Simplify(b);
		
		// a is zero -> return b
		if (a.isNumber && a.sampleValue==0f)
			return new Kdouble(b);
		// b is zero -> return a
		if (b.isNumber && b.sampleValue==0f)
			return new Kdouble(a);
		
		if (IsFirst(a.expression,b.expression)) 
			return new Kdouble(a.expression+"+"+b.expression,
				a.sampleValue+b.sampleValue,
				a.cc+b.cc+1);
		else
			return new Kdouble(b.expression+"+"+a.expression,
				a.sampleValue+b.sampleValue,
				a.cc+b.cc+1);	
	}
	
	public static Kdouble operator + (Kdouble a, float b) {
		return a+(new Kdouble(b));
	}
	
	public static Kdouble operator + (float a, Kdouble b) {
		return (new Kdouble(a))+b;
	}
	
	public static Kdouble operator - (Kdouble a, Kdouble b) {
		// both are numbers -> reduce to one number with added value
		if (a.isNumber && b.isNumber) {
			return new Kdouble(a.sampleValue+b.sampleValue);
		}
		
		a=Simplify(a);
		b=Simplify(b);
		
		// a is zero -> return b
		if (a.isNumber && a.sampleValue==0f)
			return new Kdouble(b);
		// b is zero -> return a
		if (b.isNumber && b.sampleValue==0f)
			return new Kdouble(a);
		
		return new Kdouble(a.expression+"-"+b.expression,
			a.sampleValue-b.sampleValue,
			a.cc+b.cc+1);
		
	}
	
	public static Kdouble operator - (Kdouble a, float b) {
		return a-(new Kdouble(b));
	}
	
	public static Kdouble operator - (float a, Kdouble b) {
		return (new Kdouble(a))-b;
	}
	
	public static Kdouble operator * (Kdouble a, Kdouble b) {
		// both are numbers -> reduce to one number with added value
		if (a.isNumber && b.isNumber) {
			return new Kdouble(a.sampleValue*b.sampleValue);
		}
		
		a=Simplify(a);
		b=Simplify(b);
		
		// a is one -> return b
		if (a.isNumber && a.sampleValue==1f)
			return new Kdouble(b);
		// b is one -> return a
		if (b.isNumber && b.sampleValue==1f)
			return new Kdouble(a);
		
		if (IsFirst(a.expression,b.expression)) 
			return new Kdouble(a.expression+"*"+b.expression,
				a.sampleValue*b.sampleValue,
				a.cc+b.cc+1);
		else
			return new Kdouble(b.expression+"*"+a.expression,
				a.sampleValue*b.sampleValue,
				a.cc+b.cc+1);
	}
	
	public static Kdouble operator * (Kdouble a, float b) {
		return a*(new Kdouble(b));
	}
	
	public static Kdouble operator * (float a, Kdouble b) {
		return (new Kdouble(a))*b;
	}
	
	public static Kdouble operator / (Kdouble a, Kdouble b) {
		// both are numbers -> reduce to one number with added value
		if (a.isNumber && b.isNumber) {
			return new Kdouble(a.sampleValue/b.sampleValue);
		}
		a=Simplify(a);
		b=Simplify(b);
		
		// b is one -> return a
		if (b.isNumber && b.sampleValue==1f)
			return new Kdouble(a);
		
		return new Kdouble(a.expression+"/"+b.expression,
			a.sampleValue/b.sampleValue,
			a.cc+b.cc+1);
	}
	
	public static Kdouble operator / (Kdouble a, float b) {
		return a/(new Kdouble(b));
	}
	
	public static Kdouble operator / (float a, Kdouble b) {
		return (new Kdouble(a))/b;
	}
	
	// this & that
	
	public override string ToString() {
		return expression+" = "+sampleValue.ToString()+" (CC="+cc.ToString()+")";
	}
	
	public static void PrintVariables() {
		foreach (string variableName in variables.Keys) {
			int varNo=variables[variableName];
			if (varNo!=-1) {
				Console.WriteLine ("k_"+varNo.ToString()+"="+variableName);
			}
		}
	}
	
	public static void PrintExpressions() {
		foreach (string variableName in variables.Keys) {
			int varNo=variables[variableName];
				Console.WriteLine ("No "+varNo.ToString()+": "+variableName);
		}
	}
	
	
	private static string Simplify(string e) {
		if (variables.ContainsKey(e)) {  // if yes, replace expression by variable
			int varNo=variables[e];
			if (varNo==-1) {  // assign variable number as soon as first time reused
				varNo=variablesCounter;
				variablesCounter++;
				variables[e]=varNo;
			}
			return "k_"+varNo.ToString();
		
		} else { // if not, store expression for later use as variable
			variables.Add (e,-1);
			return e;
			//Debug.Log (e);
		}
	}
	
	/// <summary>
	/// Try to substitute the expression by an existing variable
	/// </summary>
	/// <param name='e'>
	/// Expression.
	/// </param>
	public static Kdouble Simplify(Kdouble k) {
		if (k.IsNumber || k.isVariable) {
			return k;
		} else {
			string simplified=Simplify(k.expression);
			if (simplified!=k.expression)  // expression substituded by variable
				return new Kdouble(simplified,k.sampleValue,isVariable: true);
			else
				return k;
		}
	}
	
	// private methods
	
	/// <summary>
	/// Convert a float to a accuracy reduced string (not fully implemented yet)
	/// </summary>
	/// <returns>
	/// The string.
	/// </returns>
	/// <param name='value'>
	/// Value.
	/// </param>
	private static string ToString(float value) {
		return value.ToString();
	}
	
	private static bool IsFirst(string a,string b) {
		if (FastHash(a)<FastHash(b))
			return true;
		else
			return false;
	}
	
	private static int FastHash(string s) {
		
		return s.GetHashCode();
	}


	
}


// math library based on Kdouble 


/// <summary>
/// KMath is a both float and Kdouble capable replacement for Math
/// </summary>
public static class KMath {
	
	
	public static Kdouble Sin (Kdouble k) {
		if (k.IsNumber)
			return new Kdouble(Math.Sin(k.SampleValue));
		else
			return new Kdouble("Math.Sin("+Kdouble.Simplify(k).Expression+")",
				Math.Sin(k.SampleValue),
				k.Cc+1);
		
	}
	
	public static Kdouble Cos (Kdouble k) {
		if (k.IsNumber)
			return new Kdouble(Math.Cos (k.SampleValue));
		else
			return new Kdouble("Math.Cos("+Kdouble.Simplify(k).Expression+")",
				Math.Cos(k.SampleValue),
				k.Cc+1);
	}
	
	// wrapper methods for ordinary floats -> KMath can directly replace Mathf
	
	public static double Sin (this double f) {
		return Math.Sin (f);
	}
	
	public static double Cos (this double f) {
		return Math.Cos (f);
	}
	
	// helper methods
	
	// functional conditional expressions
	
	public static double IfIsSmaller(double testvalue, double comparevalue, double ifValue, double elseValue) {
		if (testvalue<comparevalue) 
			return ifValue;
		else
			return elseValue;
	}
	
	public static Kdouble IfIsSmaller(Kdouble testValue, Kdouble compareValue, Kdouble ifValue, Kdouble elseValue) {
		if (testValue.IsNumber && compareValue.IsNumber) // if the decision can be made now, do it
			if (testValue.SampleValue<compareValue.SampleValue)
				return new Kdouble(ifValue);
			else
				return new Kdouble(elseValue);
		else
			return new Kdouble("KMath.IfIsSmaller("+testValue.Expression+","+compareValue.Expression+","+ifValue.Expression+","+elseValue.Expression+")",
				IfIsSmaller(testValue.SampleValue, compareValue.SampleValue, ifValue.SampleValue, elseValue.SampleValue),
				testValue.Cc+compareValue.Cc+ifValue.Cc+elseValue.Cc+1,
				true);
	}
	
	
	public static double IfIsGreater(double testvalue, double comparevalue, double ifValue, double elseValue) {
		if (testvalue>comparevalue) 
			return ifValue;
		else
			return elseValue;
	}
	
	public static Kdouble IfIsGreater(Kdouble testValue, Kdouble compareValue, Kdouble ifValue, Kdouble elseValue) {
		if (testValue.IsNumber && compareValue.IsNumber) // if the decision can be made now, do it
			if (testValue.SampleValue>compareValue.SampleValue)
				return new Kdouble(ifValue);
			else
				return new Kdouble(elseValue);
		else
			return new Kdouble("KMath.IfIsGreater("+testValue.Expression+","+compareValue.Expression+","+ifValue.Expression+","+elseValue.Expression+")",
				IfIsGreater(testValue.SampleValue, compareValue.SampleValue, ifValue.SampleValue, elseValue.SampleValue),
				testValue.Cc+compareValue.Cc+ifValue.Cc+elseValue.Cc+1,
				true);
	}
	
}


// testing code from here

public class KdoubleTest {
	
	public KdoubleTest() {
	}
	
	public static void RunTest() {
	
		Kdouble a = new Kdouble(1);
		Kdouble b= KMath.Cos( (a+1)*3*(new Kdouble("MlVar",3.0)));
		Kdouble c= 3*b*b/24;
		
		Console.WriteLine(c.ToString ());
		Kdouble.PrintVariables();
		Kdouble.PrintExpressions();
		Console.WriteLine(c.SampleValue);
		
	}
}
