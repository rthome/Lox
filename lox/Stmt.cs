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
			T VisitFunctionStmt(Function stmt);
			T VisitIfStmt(If stmt);
			T VisitPrintStmt(Print stmt);
			T VisitReturnStmt(Return stmt);
			T VisitVarStmt(Var stmt);
			T VisitWhileStmt(While stmt);
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
		
		public class Function : Stmt
		{
			public Token Name { get; set; }
			
			public List<Token> Parameters { get; set; }
			
			public List<Stmt> Body { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitFunctionStmt(this);
			
			public Function(Token name, List<Token> parameters, List<Stmt> body)
			{
				Name = name;
				Parameters = parameters;
				Body = body;
			}
		}
		
		public class If : Stmt
		{
			public Expr Cond { get; set; }
			
			public Stmt ThenBranch { get; set; }
			
			public Stmt ElseBranch { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStmt(this);
			
			public If(Expr cond, Stmt thenBranch, Stmt elseBranch)
			{
				Cond = cond;
				ThenBranch = thenBranch;
				ElseBranch = elseBranch;
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
		
		public class Return : Stmt
		{
			public Token Keyword { get; set; }
			
			public Expr Value { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitReturnStmt(this);
			
			public Return(Token keyword, Expr value)
			{
				Keyword = keyword;
				Value = value;
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
		
		public class While : Stmt
		{
			public Expr Cond { get; set; }
			
			public Stmt Body { get; set; }
			
			public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitWhileStmt(this);
			
			public While(Expr cond, Stmt body)
			{
				Cond = cond;
				Body = body;
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
