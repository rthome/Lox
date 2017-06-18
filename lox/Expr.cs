namespace lox
{
	abstract class Expr
	{
		public abstract T Accept<T>(IVisitor<T> visitor);
		
		public interface IVisitor<T>
		{
			T VisitBinaryExpr(Binary expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitUnaryExpr(Unary expr);
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
	}
}
