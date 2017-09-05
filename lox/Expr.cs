using System;
using System.Collections.Generic;

namespace lox
{
	abstract class Expr
	{
		public abstract T Accept<T>(IVisitor<T> visitor);
		
		public interface IVisitor<T>
		{
			T VisitTernaryExpr(Ternary expr);
			T VisitBinaryExpr(Binary expr);
			T VisitCallExpr(Call expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
			T VisitAssignExpr(Assign expr);
		}
		
		public class Ternary : Expr
		{
			public Expr Cond { get; set; }
			
			public Expr Left { get; set; }
			
			public Expr Right { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitTernaryExpr(this);
			
			public Ternary(Expr cond, Expr left, Expr right)
			{
				Cond = cond;
				Left = left;
				Right = right;
			}
		}
		
		public class Binary : Expr
		{
			public Expr Left { get; set; }
			
			public Token Op { get; set; }
			
			public Expr Right { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpr(this);
			
			public Binary(Expr left, Token op, Expr right)
			{
				Left = left;
				Op = op;
				Right = right;
			}
		}
		
		public class Call : Expr
		{
			public Expr Callee { get; set; }
			
			public Token Paren { get; set; }
			
			public List<Expr> Arguments { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCallExpr(this);
			
			public Call(Expr callee, Token paren, List<Expr> arguments)
			{
				Callee = callee;
				Paren = paren;
				Arguments = arguments;
			}
		}
		
		public class Grouping : Expr
		{
			public Expr Expression { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpr(this);
			
			public Grouping(Expr expression)
			{
				Expression = expression;
			}
		}
		
		public class Literal : Expr
		{
			public object Value { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpr(this);
			
			public Literal(object value)
			{
				Value = value;
			}
		}
		
		public class Logical : Expr
		{
			public Expr Left { get; set; }
			
			public Token Op { get; set; }
			
			public Expr Right { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLogicalExpr(this);
			
			public Logical(Expr left, Token op, Expr right)
			{
				Left = left;
				Op = op;
				Right = right;
			}
		}
		
		public class Unary : Expr
		{
			public Token Op { get; set; }
			
			public Expr Right { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpr(this);
			
			public Unary(Token op, Expr right)
			{
				Op = op;
				Right = right;
			}
		}
		
		public class Variable : Expr
		{
			public Token Name { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpr(this);
			
			public Variable(Token name)
			{
				Name = name;
			}
		}
		
		public class Assign : Expr
		{
			public Token Name { get; set; }
			
			public Expr Value { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignExpr(this);
			
			public Assign(Token name, Expr value)
			{
				Name = name;
				Value = value;
			}
		}
	}
}
