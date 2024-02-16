/* * * * * * * * * * * * * *
 * A simple expression parser
 * --------------------------
 * 
 * The parser can parse a mathematical expression into a simple custom
 * expression tree. It can recognise methods and fields/contants which
 * are user extensible. It can also contain expression parameters which
 * are registrated automatically. An expression tree can be "converted"
 * into a delegate.
 * 
 * Written by Bunny83
 * 2014-11-02
 * 
 * Features:
 * - Elementary arithmetic [ + - * / ]
 * - Power [ ^ ]
 * - Brackets ( )
 * - Most function from System.Math (abs, sin, round, floor, min, ...)
 * - Constants ( e, PI )
 * - MultiValue return (quite slow, produce extra garbage each call)
 * 
 * * * * * * * * * * * * * */
using UnityEngine;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;


namespace B83//.ExpressionParser
{
    #region Enums
    /// <summary>The type of the math object this tree node is.</summary>
    public enum NodeType
    {
        Operator,
        Function,
        Value,
        Var,
        Const,
        
        Void,
        
        NumTypes
    }
    #endregion

	#region Interfaces

    public interface INode
    {
        NodeType NodeType { get; set; }
    }
	public interface IValue// : INode
	{
		#region Properties
        object Value { get; set;}
		#endregion
	}
	public interface IIdentity
	{
		#region Properties
		double IdentityAdditive { get; }
		double IdentityMultiplicitive { get; }
		#endregion
	}

	#endregion

    #region Abstract Base Classes
    public abstract class Node : INode
    {
        #region Fields
        
        string strString;
        protected NodeType eNodeType;
        
        #endregion
        
        #region Properties
        /// <summary>Gets/(sets) the type of this node.</summary>
        public NodeType NodeType
        {
            get { return this.eNodeType; }
            set { this.eNodeType = value; }
        }

		public string String
		{
			get { return strString; }
			protected set { strString = value; }
		}
        
        #endregion
        
        #region Construction
        
        //Creates an empty void node.
        protected Node()
        {
            Set ("", NodeType.Void);
        }
        
        /// <summary>Creates a void node with string str.</summary>
        protected Node(string str)
        {
            Set(str, NodeType.Void);
        }

        protected Node(NodeType nodeType)
        {
            this.NodeType = nodeType;
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>Sets the string representation and type of node.</summary>
        void Set(string str, NodeType type)
        {
            String = str;
            NodeType = type;
        }
        
        /// <summary>Another method of getting a string represenation of this node,
        /// specializing in the printed version of the node.</summary>
        //        virtual string FromString(bool bPrintVersion)
        //        {
        //            return String;
        //        }
        
        #endregion
    }

	public abstract class ValueNodeBase : Node, IValue
	{
		protected object value;

		public ValueNodeBase()
			: base(NodeType.Value)
		{
		}

		public object Value
		{
			get { return value; }
			set { this.value = value; }
		}
	}

    public abstract class ValueNode<TType> : ValueNodeBase//Node, IValue
    {
        #region Fields

		TType valueType;

        #endregion

		#region Properties
		public TType ValueType
		{
			get { return valueType; }
			set { valueType = value; }
		}
		#endregion

        public ValueNode() : base()
        {
        }

		public ValueNode(TType value) : base()
        {
			ValueType = value;
        }
    }

	public class ValueNodeScalar : ValueNode<double>
	{
		public ValueNodeScalar() : base()
		{
		}

		public ValueNodeScalar(Scalar val) : base(val)
		{
		}
	}
    #endregion

	#region Types
	
	#region Operations

	public class OperationSum : IValue
	{
		#region Fields
		//private IValue[] m_Values;
		private ValueNodeScalar[] m_Values;
		#endregion
		#region Properties
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Sum(); }
		}
		#endregion
		#region Methods
		public OperationSum(params IValue[] aValues)
		{
			// collapse unnecessary nested sum operations.
			List<ValueNodeBase> v = new List<ValueNodeBase>(aValues.Length);
			foreach (var I in aValues)
			{
				var sum = I as OperationSum;
				Debug.LogWarning("Reimplement!");
//				if (sum == null)
//					v.Add(I);
//				else
//					v.AddRange(sum.m_Values);
			}
			//m_Values = v.ToArray();
		}
		public override string ToString()
		{
			return "( " + string.Join(" + ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}
		#endregion
	}
	public class OperationProduct : IValue
	{
		#region Fields
		private IValue[] m_Values;
		#endregion
		#region Properties
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Aggregate((v1, v2) => v1 * v2); }
		}
		#endregion
		#region Methods
		public OperationProduct(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return "( " + string.Join(" * ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}
		#endregion
	}
	public class OperationPower : IValue
	{
		private IValue m_Value;
		private IValue m_Power;
		public double Value
		{
			get { return System.Math.Pow(m_Value.Value, m_Power.Value); }
		}
		public OperationPower(IValue aValue, IValue aPower)
		{
			m_Value = aValue;
			m_Power = aPower;
		}
		public override string ToString()
		{
			return "( " + m_Value + "^" + m_Power + " )";
		}
		
	}
	public class OperationNegate : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return -m_Value.Value; }
		}
		public OperationNegate(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( -" + m_Value + " )";
		}
		
	}
	public class OperationReciprocal : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return 1.0 / m_Value.Value; }
		}
		public OperationReciprocal(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( 1/" + m_Value + " )";
		}
		
	}
	public class OperationModulus : IValue
	{
		private IValue m_Value;
		private IValue m_ModValue;
		public double Value
		{
			get { return m_Value.Value % m_ModValue.Value; }
		}
		public OperationModulus(IValue aValue, IValue aModValue)
		{
			m_Value = aValue;
			m_ModValue = aModValue;
		}
		public override string ToString()
		{
			return "( " + m_Value + "%" + m_ModValue + " )";
		}
		
	}

	#endregion

	public class Scalar : IValue, IIdentity
	{
		#region Fields
		private double m_Value;
		#endregion

		#region Construction
		public Scalar()
		{
			m_Value = 1;
		}
		
		public Scalar(double aValue)
		{
			m_Value = aValue;
		}
		#endregion

		#region Methods
		public override string ToString()
		{
			return "" + m_Value + "";
		}
		#endregion

        #region IValue
        public object Value
        {
			get { return m_Value; }
			set { m_Value = (double)value; }
        }

		public double ValueD
		{
			get { return m_Value; }
			set {m_Value = value; }
		}
        #endregion

		#region IIdentity
		public double IdentityAdditive
		{
			get { return 0.0; }
		}
		
		public double IdentityMultiplicitive
		{
			get { return 1.0; }
		}
		#endregion

        #region Implicit conversion
		public static implicit operator double(Scalar n)
		{
			return n.ValueD;
		}
        #endregion
	}

//    public class Vector : MultiParameterList, IIdentity
//    {
//
//    }
//
//	public class Matrix<TRankM, TRankN> : IIdentity
//	{
//	}

    //public class Matrix<int, int> 

	public class MultiParameterList : IValue
	{
		private IValue[] m_Values;
		public IValue[] Parameters { get { return m_Values; } }
		public double Value
		{
			get { return m_Values.Select(v => v.Value).FirstOrDefault(); }
		}
		public MultiParameterList(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return string.Join(", ", m_Values.Select(v => v.ToString()).ToArray());
		}
	}
	
	public class CustomFunction : IValue
	{
		private IValue[] m_Params;
		private System.Func<double[], double> m_Delegate;
		private string m_Name;
		public double Value
		{
			get
			{
				if (m_Params == null)
					return m_Delegate(null);
				return m_Delegate(m_Params.Select(p => p.Value).ToArray());
			}
		}
		public CustomFunction(string aName, System.Func<double[], double> aDelegate, params IValue[] aValues)
		{
			m_Delegate = aDelegate;
			m_Params = aValues;
			m_Name = aName;
		}
		public override string ToString()
		{
			if (m_Params == null)
				return m_Name;
			return m_Name + "( " + string.Join(", ", m_Params.Select(v => v.ToString()).ToArray()) + " )";
		}
	}
	public class Parameter : Scalar
	{
		public enum TypeOfRange { Unbounded, Bounded }
		public TypeOfRange RangeType = TypeOfRange.Unbounded;
		Scalar min, max;
		public Scalar Min
		{
			get { return min; }
			set { min = (Max != 0 ? new Scalar(System.Math.Min(value, Max)) : value); }
		}
		public Scalar Max
		{
			get { return max; }
			set { max = new Scalar(System.Math.Max(Min, value)); }
		}

		public string Name { get; private set; }
		public override string ToString()
		{
			return Name+"["+base.ToString()+"]";
		}
		public Parameter(string aName) : base(0)
		{
			Name = aName;
			min = new Scalar(Mathf.NegativeInfinity);
			max = new Scalar(Mathf.Infinity);
		}
	}
	
	public class Expression : IValue
	{
		public Dictionary<string, Parameter> Parameters = new Dictionary<string, Parameter>();
		public IValue ExpressionTree { get; set; }
		public double Value
		{
			get { return ExpressionTree.Value; }
		}
		public double[] MultiValue
		{
			get {
				var t = ExpressionTree as MultiParameterList;
				if (t != null)
				{
					double[] res = new double[t.Parameters.Length];
					for (int i = 0; i < res.Length; i++)
						res[i] = t.Parameters[i].Value;
					return res;
				}
				return null;
			}
		}
		public override string ToString()
		{
			return ExpressionTree.ToString();
		}
		public ExpressionDelegate ToDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for(int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();
			
			return (p) => Invoke(p, parameters2);
		}
		public MultiResultDelegate ToMultiResultDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for (int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();
			
			
			return (p) => InvokeMultiResult(p, parameters2);
		}
		double Invoke(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++ )
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return Value;
		}
		double[] InvokeMultiResult(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++)
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return MultiValue;
		}
		public static Expression Parse(string aExpression)
		{
			return new ExpressionParser().EvaluateExpression(aExpression);
		}
		
		public class ParameterException : System.Exception { public ParameterException(string aMessage) : base(aMessage) { } }
	}
	public delegate double ExpressionDelegate(params double[] aParams);
	public delegate double[] MultiResultDelegate(params double[] aParams);

	#endregion
	
	public class ExpressionParser
	{
		private List<string> m_BracketHeap = new List<string>();
		private Dictionary<string, System.Func<double>> m_Consts = new Dictionary<string, System.Func<double>>();
		private Dictionary<string, System.Func<double[], double>> m_Funcs = new Dictionary<string, System.Func<double[], double>>();
		private Expression m_Context;

		public bool UseMathf = false;
		
		public ExpressionParser()
		{
			AddConstants();
			AddFunctions();
		}

		void AddFunctions()
		{
			System.Random rnd = new System.Random();

			if (UseMathf)
			{
				//				m_Funcs.Add("sqrt", (p) => Mathf.Sqrt(p.FirstOrDefault()));
				//				m_Funcs.Add("abs", (p) => Mathf.Abs(p.FirstOrDefault()));
				//				m_Funcs.Add("ln", (p) => Mathf.Log(p.FirstOrDefault()));
				//				m_Funcs.Add("floor", (p) => Mathf.Floor(p.FirstOrDefault()));
				//				m_Funcs.Add("floorToInt", (p) => Mathf.FloorToInt(p.FirstOrDefault()));
				//				m_Funcs.Add("ceil", (p) => Mathf.Ceil(p.FirstOrDefault()));
				//				m_Funcs.Add("ceilToInt", (p) => Mathf.CeilToInt(p.FirstOrDefault()));
				//				m_Funcs.Add("round", (p) => Mathf.Round(p.FirstOrDefault()));
				//				m_Funcs.Add("roundToInt", (p) => Mathf.RoundToInt(p.FirstOrDefault()));
				//				m_Funcs.Add("exp", (p) => Mathf.Exp(p.FirstOrDefault()));
				//				m_Funcs.Add("sgn", (p) => Mathf.Sign(p.FirstOrDefault()));
				//				
				//				m_Funcs.Add("sin", (p) => Mathf.Sin(p.FirstOrDefault()));
				//				m_Funcs.Add("sind", (p) => Mathf.Sin(p.FirstOrDefault() * Mathf.Deg2Rad));
				//				m_Funcs.Add("cos", (p) => Mathf.Cos(p.FirstOrDefault()));
				//				m_Funcs.Add("cosd", (p) => Mathf.Cos(p.FirstOrDefault() * Mathf.Deg2Rad));
				//				m_Funcs.Add("tan", (p) => Mathf.Tan(p.FirstOrDefault()));
				//				m_Funcs.Add("tand", (p) => Mathf.Tan(p.FirstOrDefault() * Mathf.Deg2Rad));
				//				
				//				m_Funcs.Add("asin", (p) => Mathf.Asin(p.FirstOrDefault()));
				//				m_Funcs.Add("acos", (p) => Mathf.Acos(p.FirstOrDefault()));
				//				m_Funcs.Add("atan", (p) => Mathf.Atan(p.FirstOrDefault()));
				//				m_Funcs.Add("atan2", (p) => Mathf.Atan2(p.FirstOrDefault(),p.ElementAtOrDefault(1)));
				//				//System.Math.Floor
				//				m_Funcs.Add("min", (p) => Mathf.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
				//				m_Funcs.Add("max", (p) => Mathf.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
				//				m_Funcs.Add("rnd", (p) =>
				//				            {
				//					if (p.Length == 2)
				//						return p[0] + /*rnd.NextDouble()*/Random.value * (p[1] - p[0]);
				//					if (p.Length == 1)
				//						return /*rnd.NextDouble()*/Random.value * p[0];
				//					return /*rnd.NextDouble()*/Random.value;
				//				});
			}
			else
			{
				m_Funcs.Add("sqrt", (p) => System.Math.Sqrt(p.FirstOrDefault()));
				m_Funcs.Add("abs", (p) => System.Math.Abs(p.FirstOrDefault()));
				m_Funcs.Add("ln", (p) => System.Math.Log(p.FirstOrDefault()));
				m_Funcs.Add("floor", (p) => System.Math.Floor(p.FirstOrDefault()));
				//m_Funcs.Add("floorToInt", (p) => (double)Mathf.FloorToInt((float)p.FirstOrDefault()));
				m_Funcs.Add("ceil", (p) => System.Math.Ceiling(p.FirstOrDefault()));
				//m_Funcs.Add("ceilToInt", (p) => (double)Mathf.CeilToInt((float)p.FirstOrDefault()));
				m_Funcs.Add("round", (p) =>
				{
					if (p.Length == 2)
						return System.Math.Round(p.FirstOrDefault(), (int)p.ElementAtOrDefault(1));
					else
						return System.Math.Round(p.FirstOrDefault());
				});
				//m_Funcs.Add("roundToInt", (p) => System.Math.RoundToInt(p.FirstOrDefault()));
				m_Funcs.Add("exp", (p) => System.Math.Exp(p.FirstOrDefault()));
				m_Funcs.Add("log", (p) => 
				{
					if (p.Length == 2)
						return System.Math.Log(p.FirstOrDefault(), p.ElementAtOrDefault(1));
					else
						return System.Math.Log(p.FirstOrDefault());
				});
				m_Funcs.Add("log10",(p)=> System.Math.Log10(p.FirstOrDefault()));
				m_Funcs.Add("sgn", (p) => System.Math.Sign(p.FirstOrDefault()));
				
				m_Funcs.Add("sin", (p) => System.Math.Sin(p.FirstOrDefault()));
				m_Funcs.Add("sind", (p) => System.Math.Sin(p.FirstOrDefault() * Mathf.Deg2Rad));
				m_Funcs.Add("sinh", (p) => System.Math.Sinh(p.FirstOrDefault()));
				m_Funcs.Add("cos", (p) => System.Math.Cos(p.FirstOrDefault()));
				m_Funcs.Add("cosd", (p) => System.Math.Cos(p.FirstOrDefault() * Mathf.Deg2Rad));
				m_Funcs.Add("cosh", (p) => System.Math.Cosh(p.FirstOrDefault()));
				m_Funcs.Add("tan", (p) => System.Math.Tan(p.FirstOrDefault()));
				m_Funcs.Add("tand", (p) => System.Math.Tan(p.FirstOrDefault() * Mathf.Deg2Rad));
				m_Funcs.Add("tanh", (p) => System.Math.Tanh(p.FirstOrDefault()));
				
				m_Funcs.Add("asin", (p) => System.Math.Asin(p.FirstOrDefault()));
				m_Funcs.Add("acos", (p) => System.Math.Acos(p.FirstOrDefault()));
				m_Funcs.Add("atan", (p) => System.Math.Atan(p.FirstOrDefault()));
				m_Funcs.Add("atan2", (p) => System.Math.Atan2(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
				//System.Math.Floor
				m_Funcs.Add("min", (p) => System.Math.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
				m_Funcs.Add("max", (p) => System.Math.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
				//m_Funcs.Add("mod", (p) => System.Math.Mod());
				m_Funcs.Add("rnd", (p) =>
				{
					if (p.Length == 2)
						return p.FirstOrDefault() + rnd.NextDouble() * (p.ElementAtOrDefault(1) - p.FirstOrDefault());
					if (p.Length == 1)
						return rnd.NextDouble() * p.FirstOrDefault();
					return rnd.NextDouble();
				});
				m_Funcs.Add("fact", (p) => Factorial((long)p.FirstOrDefault()));
				m_Funcs.Add("binom", (p) => Binomial((uint)p.FirstOrDefault(), (uint)p.ElementAtOrDefault(1)));
			}
		}

		void AddConstants ()
		{
			m_Consts.Add("PI", () => Mathf.PI);
			m_Consts.Add("e", () => System.Math.E);
			m_Consts.Add("Infinity", () => Mathf.Infinity);
			m_Consts.Add("NegInfinity", () => Mathf.NegativeInfinity);
			m_Consts.Add("Deg2Rad", () => Mathf.Deg2Rad);
			m_Consts.Add("Rad2Deg", () => Mathf.Rad2Deg);
		}

		public long Factorial(long n)
		{
			//n = (int)System.Math.Floor(n);
			if (n < 0) return 0;
			if (n == 0 || n == 1) return 1;
			else return n * Factorial(n - 1);
		}

		public int Binomial(uint n, uint k)
		{
			if (n == 0 || k == 0 || k == n) return 1;
			if (k > n) return 0;
			return Binomial(n - 1, k - 1) + Binomial(n - 1, k);
		}

		public Dictionary<string, System.Func<double>> Constants
		{
			get { return m_Consts; }
		}

		public string[] ConstantNames
		{
			get { return m_Consts.Keys.ToArray(); }
		}

		public Dictionary<string, System.Func<double[], double>> Functions
		{
			get { return m_Funcs; }
		}

		public string[] FunctionNames
		{
			get { return m_Funcs.Keys.ToArray(); }
		}

		public Dictionary<string, Parameter> Variables
		{
			get { return m_Context == null ? null : m_Context.Parameters; }
		}

		public string[] VariableNames
		{
			get
			{
				if (m_Context == null) return null;
				return m_Context.Parameters.Keys.ToArray();
			}
		}

		public void AddFunc(string aName, System.Func<double[],double> aMethod)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs[aName] = aMethod;
			else
				m_Funcs.Add(aName, aMethod);
		}
		
		public void AddConst(string aName, System.Func<double> aMethod)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts[aName] = aMethod;
			else
				m_Consts.Add(aName, aMethod);
		}
		public void RemoveFunc(string aName)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs.Remove(aName);
		}
		public void RemoveConst(string aName)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts.Remove(aName);
		}
		
		int FindClosingBracket(ref string aText, int aStart, char aOpen, char aClose)
		{
			int counter = 0;
			for (int i = aStart; i < aText.Length; i++)
			{
				if (aText[i] == aOpen)
					counter++;
				if (aText[i] == aClose)
					counter--;
				if (counter == 0)
					return i;
			}
			return -1;
		}
		
		void SubstituteBracket(ref string aExpression, int aIndex)
		{
			int closing = FindClosingBracket(ref aExpression, aIndex, '(', ')');
			if (closing > aIndex + 1)
			{
				string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1);
				m_BracketHeap.Add(inner);
				string sub = "&" + (m_BracketHeap.Count - 1) + ";";
				aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
			}
			else throw new ParseException("Bracket not closed!");
		}
		
		IValue Parse(string aExpression)
		{
			aExpression = aExpression.Trim();
			int index = aExpression.IndexOf('(');
			while (index >= 0)
			{
				SubstituteBracket(ref aExpression, index);
				index = aExpression.IndexOf('(');
			}
			if (aExpression.Contains(','))
			{
				string[] parts = aExpression.Split(',');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				return new MultiParameterList(exp.ToArray());
			}
			else if (aExpression.Contains('+'))
			{
				string[] parts = aExpression.Split('+');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('-'))
			{
				string[] parts = aExpression.Split('-');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationNegate(Parse(s)));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('*'))
			{
				string[] parts = aExpression.Split('*');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					exp.Add(Parse(parts[i]));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('/'))
			{
				string[] parts = aExpression.Split('/');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationReciprocal(Parse(s)));
				}
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('%'))
			{
//				string[] parts = aExpression.Split('%');
//				List<IValue> exp = new List<IValue>(parts.Length);
//				for (int i = 0; i < parts.Length; i++)
//					exp.Add(Parse(parts[i]));
//				if (exp.Count == 1)
//					return exp[0];
//				return new OperationModulus(
				int pos = aExpression.IndexOf('%');
				var val = Parse(aExpression.Substring(0, pos));
				var mod = Parse(aExpression.Substring(pos + 1));
				return new OperationModulus(val, mod);
			}
			else if (aExpression.Contains('^'))
			{
				int pos = aExpression.IndexOf('^');
				var val = Parse(aExpression.Substring(0, pos));
				var pow = Parse(aExpression.Substring(pos + 1));
				return new OperationPower(val, pow);
			}
			bool funcSkipped = false;
			foreach (var M in m_Funcs)
			{
				if (aExpression.StartsWith(M.Key))
				{
					bool skipFunc = false;

					foreach (var m in m_Funcs)
					{
						if (m.Key == M.Key)
							continue;

						if (aExpression.StartsWith(m.Key) && !funcSkipped)
						{
							skipFunc = funcSkipped = true;
							break;
						}
					}

					if (skipFunc)
						continue;

					funcSkipped = false;

					var inner = aExpression.Substring(M.Key.Length);
					var param = Parse(inner);
					var multiParams = param as MultiParameterList;
					IValue[] parameters;
					if (multiParams != null)
						parameters = multiParams.Parameters;
					else
					parameters = new IValue[] { param };
					return new CustomFunction(M.Key, M.Value, parameters);
				}
			}
			foreach (var C in m_Consts)
			{
				if (aExpression.StartsWith(C.Key))
				{
					return new CustomFunction(C.Key,(p)=>C.Value(),null);
				}
			}
			int index2a = aExpression.IndexOf('&');
			int index2b = aExpression.IndexOf(';');
			if (index2a >= 0 && index2b >= 2)
			{
				var inner = aExpression.Substring(index2a + 1, index2b - index2a - 1);
				int bracketIndex;
				if (int.TryParse(inner, out bracketIndex) && bracketIndex >= 0 && bracketIndex < m_BracketHeap.Count)
				{
					return Parse(m_BracketHeap[bracketIndex]);
				}
				else
					throw new ParseException("Can't parse substitude token");
			}
			double doubleValue;
			if (double.TryParse(aExpression, out doubleValue))
			{
				return new Scalar(doubleValue);
			}
			if (ValidIdentifier(aExpression))
			{
				if (m_Context.Parameters.ContainsKey(aExpression))
					return m_Context.Parameters[aExpression];
				var val = new Parameter(aExpression);
				m_Context.Parameters.Add(aExpression, val);
				return val;
			}
			
			throw new ParseException("Reached unexpected end within the parsing tree");
		}
		
		private bool ValidIdentifier(string aExpression)
		{
			aExpression = aExpression.Trim();
			if (string.IsNullOrEmpty(aExpression))
				return false;
			if (aExpression.Length < 1)
				return false;
			if (aExpression.Contains(" "))
				return false;
			if (!"abcdefghijklmnopqrstuvwxyz§$".Contains(char.ToLower(aExpression[0])))
				return false;
			if (m_Consts.ContainsKey(aExpression))
				return false;
			if (m_Funcs.ContainsKey(aExpression))
				return false;
			return true;
		}
		
		public Expression EvaluateExpression(string aExpression)
		{
			var val = new Expression();
			m_Context = val;
			val.ExpressionTree = Parse(aExpression);
			m_Context = null;
			m_BracketHeap.Clear();
			return val;
		}
		
		public double Evaluate(string aExpression)
		{
			return EvaluateExpression(aExpression).Value;
		}
		public static double Eval(string aExpression)
		{
			return new ExpressionParser().Evaluate(aExpression);
		}
		
		public class ParseException : System.Exception { public ParseException(string aMessage) : base(aMessage) { } }
	}
}