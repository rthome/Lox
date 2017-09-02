using System;
using static lox.TokenType;

namespace lox
{
    class Interpreter : Expr.IVisitor<object>
    {
        bool IsTruthy(object value)
        {
            if (value is null)
                return false;
            if (value is bool boolValue)
                return boolValue;
            return true;
        }

        bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
                return true;
            if (a == null)
                return false;
            return a.Equals(b);
        }

        void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double)
                return;
            throw new RuntimeException(op, "Operand must be a number.");
        }

        void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double)
                return;
            throw new RuntimeException(op, "Operands must be numbers.");
        }

        string Stringify(object value)
        {
            if (value == null)
                return "nil";
            
            return value.ToString();
        }

        object Evaluate(Expr expr) => expr.Accept(this);

        object Expr.IVisitor<object>.VisitLiteralExpr(Expr.Literal expr) => expr.Value;

        object Expr.IVisitor<object>.VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expression);

        object Expr.IVisitor<object>.VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);
            switch(expr.Op.Type)
            {
                case Minus:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
                case Bang:
                    return !IsTruthy(right);
            }

            // Unreachable
            return null;
        }

        object Expr.IVisitor<object>.VisitBinaryExpr(Expr.Binary expr)
        {
            var right = Evaluate(expr.Right);
            var left = Evaluate(expr.Left);
            switch(expr.Op.Type)
            {
                case Minus:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case Slash:
                    CheckNumberOperands(expr.Op, left, right);
                    if ((double)right == 0)
                        throw new RuntimeException(expr.Op, "Division by zero.");
                    return (double)left / (double)right;
                case Star:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case Plus:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    if (left is string && right is string)
                        return (string)left + (string)right;
                    throw new RuntimeException(expr.Op, "Operands must be two numbers or two strings.");
                case Greater:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case GreaterEqual:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case Less:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case LessEqual:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case BangEqual:
                    return !IsEqual(left, right);
                case EqualEqual:
                    return IsEqual(left, right);
            }

            // Unreachable
            return null;
        }

        object Expr.IVisitor<object>.VisitTernaryExpr(Expr.Ternary expr)
        {
            throw new NotImplementedException();
        }

        public void Interpret(Expr expression)
        {
            try
            {
                var value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeException exc)
            {
                Program.RuntimeError(exc);
            }
        }
    }
}