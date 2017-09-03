using System.Text;

namespace lox
{
    class AstPrinter : Expr.IVisitor<string>
    {
        string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();
            sb.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                sb.Append(" ");
                sb.Append(expr.Accept(this));
            }
            sb.Append(")");

            return sb.ToString();
        }

        public string Print(Expr expr) => expr.Accept(this);

        string Expr.IVisitor<string>.VisitTernaryExpr(Expr.Ternary expr)
        {
            return Parenthesize("?:", expr.Cond, expr.Left, expr.Right);
        }

        string Expr.IVisitor<string>.VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        string Expr.IVisitor<string>.VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        string Expr.IVisitor<string>.VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null)
                return "nil";
            if (expr.Value is string)
                return $"\"{expr.Value}\"";
            return expr.Value.ToString();
        }

        string Expr.IVisitor<string>.VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        string Expr.IVisitor<string>.VisitVariableExpr(Expr.Variable expr)
        {
            return expr.Name.Lexeme;
        }
    }
}
