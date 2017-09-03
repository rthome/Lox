namespace lox
{
	abstract class Stmt
	{
		public abstract T Accept<T>(IVisitor<T> visitor);
		
		public interface IVisitor<T>
		{
			T VisitExpressionStmt(Expression stmt);
			T VisitPrintStmt(Print stmt);
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
	}
}
