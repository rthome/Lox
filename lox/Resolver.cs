using System;
using System.Collections.Generic;
using Void = lox.Util.Void;

namespace lox
{
    class Resolver : Expr.IVisitor<Void>, Stmt.IVisitor<Void>
    {
        sealed class ScopeHelper : IDisposable
        {
            readonly Resolver resolver;

            public void Dispose()
            {
                resolver.EndScope();
            }

            public ScopeHelper(Resolver resolver)
            {
                this.resolver = resolver;
            }
        }

        enum FunctionType
        {
            None,
            Function,
        }

        readonly Interpreter interpreter;
        readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();

        FunctionType currentFunction = FunctionType.None;

        ScopeHelper BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
            return new ScopeHelper(this);
        }

        void EndScope() => scopes.Pop();

        void Declare(Token name)
        {
            if (scopes.TryPeek(out var scope))
            {
                if (scope.ContainsKey(name.Lexeme))
                    Program.Error(name, "Variable with this name already declared in this scope.");
                scope[name.Lexeme] = false;
            }
        }

        void Define(Token name)
        {
            if (scopes.TryPeek(out var scope))
                scope[name.Lexeme] = true;
        }

        void ResolveLocal(Expr expr, Token name)
        {
            var distance = 0;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, distance);
                    break;
                }

                distance++;
            }
        }

        void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            var enclosingFunction = currentFunction;
            currentFunction = type;

            using (BeginScope())
            {
                foreach (var param in function.Parameters)
                {
                    Declare(param);
                    Define(param);
                }
                Resolve(function.Body);
            }

            currentFunction = enclosingFunction;
        }

        void Resolve(Expr expr) => expr.Accept(this);

        void Resolve(Stmt statement) => statement.Accept(this);

        public void Resolve(IEnumerable<Stmt> statements)
        {
            foreach (var statement in statements)
                Resolve(statement);
        }

        Void Expr.IVisitor<Void>.VisitTernaryExpr(Expr.Ternary expr)
        {
            Resolve(expr.Cond);
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);
            foreach (var argument in expr.Arguments)
                Resolve(argument);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        Void Expr.IVisitor<Void>.VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.TryPeek(out var scope) && scope.TryGetValue(expr.Name.Lexeme, out var defined) && !defined)
                Program.Error(expr.Name, "Cannot read locale variable in its own initializer.");
            ResolveLocal(expr, expr.Name);
            return null;
        }

        Void Expr.IVisitor<Void>.VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitBreakStmt(Stmt.Break stmt)
        {
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.Function);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Cond);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
                Resolve(stmt.ElseBranch);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.None)
                Program.Error(stmt.Keyword, "Cannot return from top-level code.");
            if (stmt.Value != null)
                Resolve(stmt.Value);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);
            Define(stmt.Name);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Cond);
            Resolve(stmt.Body);
            return null;
        }

        Void Stmt.IVisitor<Void>.VisitBlockStmt(Stmt.Block stmt)
        {
            using (BeginScope())
                Resolve(stmt.Statements);
            return null;
        }

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }
    }
}
