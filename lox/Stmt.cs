using System;
using System.Collections.Generic;

namespace lox
{
	abstract class Stmt
	{
		public abstract T Accept<T>(IVisitor<T> visitor);
		
		public interface IVisitor<T>
		{
			T VisitExpressionStmt(Expression stmt);
			T VisitPrintStmt(Print stmt);
			T VisitVarStmt(Var stmt);
			T VisitBlockStmt(Block stmt);
		}
		
		public class Expression : Stmt
		{
			public Expr Expr { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStmt(this);
			
			public Expression(Expr expr)
			{
				Expr = expr;
			}
		}
		
		public class Print : Stmt
		{
			public Expr Expr { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitPrintStmt(this);
			
			public Print(Expr expr)
			{
				Expr = expr;
			}
		}
		
		public class Var : Stmt
		{
			public Token Name { get; set; }
			
			public Expr Initializer { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVarStmt(this);
			
			public Var(Token name, Expr initializer)
			{
				Name = name;
				Initializer = initializer;
			}
		}
		
		public class Block : Stmt
		{
			public List<Stmt> Statements { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBlockStmt(this);
			
			public Block(List<Stmt> statements)
			{
				Statements = statements;
			}
		}
	}
}
