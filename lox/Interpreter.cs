using System;
using System.Collections.Generic;
using static lox.TokenType;

namespace lox
{
    class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        readonly Environment environment = new Environment();

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

        void Execute(Stmt statement) => statement.Accept(this);

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

        object Expr.IVisitor<object>.VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Lookup(expr.Name);
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
            var condValue = Evaluate(expr.Cond);
            if (IsTruthy(condValue))
                return Evaluate(expr.Left);
            else
                return Evaluate(expr.Right);
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));
            return null;
        }

        object Stmt.IVisitor<object>.VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
                value = Evaluate(stmt.Initializer);

            environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        object Expr.IVisitor<object>.VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);
            environment.Assign(expr.Name, value);
            return value;
        }

        public void Interpret(IEnumerable<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                    Execute(stmt);
            }
            catch (RuntimeException exc)
            {
                Program.RuntimeError(exc);
            }
        }
    }
}