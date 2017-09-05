using System;
using System.Collections.Generic;
using static lox.TokenType;

namespace lox
{
    class Parser
    {
        readonly List<Token> tokens = new List<Token>();
        int current = 0;

        #region Helper Members

        Token Peek => tokens[current];

        Token Previous => tokens[current - 1];

        bool IsAtEnd => Peek.Type == EOF;

        Token Advance()
        {
            if (!IsAtEnd)
                current++;
            return Previous;
        }

        void Regurgitate()
        {
            if (current > 0)
                current--;
        }

        bool Check(TokenType type)
        {
            if (IsAtEnd)
                return false;
            return Peek.Type == type;
        }

        bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        ParseErrorException Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseErrorException();
        }

        void Synchronize()
        {
            Advance();
            while (!IsAtEnd)
            {
                if (Previous.Type == Semicolon)
                    return;

                switch (Peek.Type)
                {
                    case Class:
                    case Fun:
                    case Var:
                    case For:
                    case If:
                    case While:
                    case Print:
                    case Return:
                        return;
                }

                Advance();
            }
        }

        Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();
            throw Error(Peek, message);
        }

        Expr ConsumeBinaryOperatorNoLeft(Func<Expr> nextRule, params TokenType[] matchedTypes)
        {
            if (Match(matchedTypes))
            {
                // Hack for unary minus
                if (Previous.Type == Minus)
                {
                    Regurgitate();
                    return Unary();
                }

                var op = Previous;
                var right = nextRule();

                throw Error(op, "Binary operator appeared at start of expression (without left operand.)");
            }
            else
                return nextRule();
        }

        Expr ConsumeBinaryOperator(Func<Expr> nextRule, Func<Expr, Token, Expr, Expr> ctor, params TokenType[] matchedTypes)
        {
            var expr = ConsumeBinaryOperatorNoLeft(nextRule, matchedTypes);
            while (Match(matchedTypes))
            {
                var op = Previous;
                var right = nextRule();
                expr = ctor(expr, op, right);
            }
            return expr;
        }

        Expr ConsumeLogical(Func<Expr> nextRule, params TokenType[] matchedTypes)
        {
            return ConsumeBinaryOperator(nextRule, (l, o, r) => new Expr.Logical(l, o, r), matchedTypes);
        }

        Expr ConsumeBinary(Func<Expr> nextRule, params TokenType[] matchedTypes)
        {
            return ConsumeBinaryOperator(nextRule, (l, o, r) => new Expr.Binary(l, o, r), matchedTypes);
        }

        #endregion

        #region Grammar Rules

        Expr Primary()
        {
            if (Match(False))
                return new Expr.Literal(false);
            if (Match(True))
                return new Expr.Literal(true);
            if (Match(Nil))
                return new Expr.Literal(null);

            if (Match(Number, TokenType.String))
                return new Expr.Literal(Previous.Literal);

            if (Match(Identifier))
                return new Expr.Variable(Previous);

            if (Match(LeftParen))
            {
                var expr = Expression();
                Consume(RightParen, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek, "Expected expression.");
        }

        Expr Unary()
        {
            if (Match(Bang, Minus))
            {
                var op = Previous;
                var right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        Expr Factor() => ConsumeBinary(Unary, Slash, Star);

        Expr Term() => ConsumeBinary(Factor, Minus, Plus);

        Expr Comparison() => ConsumeBinary(Term, Greater, GreaterEqual, Less, LessEqual);

        Expr Equality() => ConsumeBinary(Comparison, BangEqual, EqualEqual);

        Expr Ternary()
        {
            var cond = Equality();
            if (Match(TokenType.QuestionMark))
            {
                var left = Equality();
                Consume(Colon, "Expected ':' in ternary operator expression.");
                var right = Equality();
                return new Expr.Ternary(cond, left, right);
            }
            return cond;
        }

        Expr Comma() => ConsumeBinary(Ternary, TokenType.Comma);

        Expr And() => ConsumeLogical(Comma, TokenType.And);

        Expr Or() => ConsumeLogical(And, TokenType.Or);

        Expr Assignment()
        {
            var expr = Or();

            if (Match(Equal))
            {
                var equals = Previous;
                var value = Assignment();

                if (expr is Expr.Variable variable)
                {
                    var name = variable.Name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        Expr Expression() => Assignment();

        Stmt ForStatement()
        {
            Consume(LeftParen, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(Semicolon))
                initializer = null;
            else if (Match(Var))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();

            Expr condition = null;
            if (!Check(Semicolon))
                condition = Expression();
            Consume(Semicolon, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(RightParen))
                increment = Expression();
            Consume(RightParen, "Expect ')' after for clause.");

            var body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    body,
                    new Stmt.Expression(increment),
                });
            }

            if (condition == null)
                condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
                body = new Stmt.Block(new List<Stmt> { initializer, body });

            return body;
        }

        Stmt IfStatement()
        {
            Consume(LeftParen, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(RightParen, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(Else))
                elseBranch = Statement();

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        Stmt PrintStatement()
        {
            var value = Expression();
            Consume(Semicolon, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        Stmt ExpressionStatement()
        {
            var value = Expression();
            Consume(Semicolon, "Expect ';' after value.");
            return new Stmt.Expression(value);
        }

        List<Stmt> Block()
        {
            var statements = new List<Stmt>();
            while (!Check(RightBrace) && !IsAtEnd)
                statements.Add(Declaration());
            Consume(RightBrace, "Expect '}' at end of block.");
            return statements;
        }

        Stmt Statement()
        {
            if (Match(For))
                ForStatement();
            if (Match(If))
                return IfStatement();
            if (Match(Print))
                return PrintStatement();
            if (Match(While))
                return WhileStatement();
            if (Match(LeftBrace))
                return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        Stmt VarDeclaration()
        {
            var name = Consume(Identifier, "Expect variable name.");
            Expr initializer = null;
            if (Match(Equal))
                initializer = Expression();

            Consume(Semicolon, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        Stmt WhileStatement()
        {
            Consume(LeftParen, "Expect '(' after 'while'.");
            var cond = Expression();
            Consume(RightParen, "Expect ')' after condition.");
            var body = Statement();

            return new Stmt.While(cond, body);
        }

        Stmt Declaration()
        {
            try
            {
                if (Match(Var))
                    return VarDeclaration();

                return Statement();
            }
            catch (ParseErrorException)
            {
                Synchronize();
                return null;
            }
        }

        #endregion

        public IEnumerable<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd)
                statements.Add(Declaration());
            return statements;
        }

        public Parser(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
        }
    }
}