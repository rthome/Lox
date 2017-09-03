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
            while(!IsAtEnd)
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

        Expr ConsumeBinaryNoLeft(Func<Expr> nextRule, params TokenType[] matchedTypes)
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

        Expr ConsumeBinary(Func<Expr> nextRule, params TokenType[] matchedTypes)
        {
            var expr = ConsumeBinaryNoLeft(nextRule, matchedTypes);
            while (Match(matchedTypes))
            {
                var op = Previous;
                var right = nextRule();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
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

        Expr Expression() => Comma();

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

        Stmt Statement()
        {
            if (Match(Print))
                return PrintStatement();

            return ExpressionStatement();
        }

        #endregion

        public IEnumerable<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd)
                statements.Add(Statement());
            return statements;
        }

        public Parser(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
        }
    }
}