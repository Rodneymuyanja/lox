﻿using lox.src.Interfaces;

namespace lox.src
{
    public abstract class Expr
    {
        public interface IVisitor<T>
        {
            public T VisitLiteralExpr(Expr.Literal expr);
            public T VisitGroupExpr(Expr.Grouping expr);
            public T VisitUnaryExpr(Expr.Unary expr);
            public T VisitBinaryExpr(Expr.Binary expr);
            public T VisitVariableExpr(Expr.Variable expr);
            public T VisitAssignmentExpr(Expr.Assign expr);
            public T VisitLogicalExpr(Expr.Logical expr);
            public T VisitCallExpr(Expr.Call expr);
            public T VisitGetExpr(Expr.Get expr);
            public T VisitSetExpr(Expr.Set expr);
            public T VisitThisExpr(Expr.This expr);
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
        public class Binary(Expr _left, Token _operator, Expr _right) : Expr
        {
            public Expr left = _left;
            public Token __operator = _operator;
            public Expr right = _right;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }
        public class Grouping(Expr _expression) : Expr
        {
            public Expr expression = _expression;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupExpr(this);
            }
        }
        public class Literal(object _value) : Expr
        {
            public object value = _value;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }
        public class Unary(Token _operator, Expr _right) : Expr
        {
            public Token __operator = _operator;
            public Expr right = _right;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

        public class Variable(Token _name) : Expr
        {
            public Token name = _name;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }

        public class Assign(Token _name, Expr _value) : Expr
        {
            public Token name = _name;
            public Expr value = _value;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitAssignmentExpr(this);
            }
        }

        public class Logical(Expr _left, Token _operator, Expr _right) : Expr
        {
            public Expr left = _left;
            public Token op = _operator;
            public Expr right = _right;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class Call(Expr _callee, Token _paren, List<Expr> _arguments) : Expr
        {
            public Expr callee = _callee;
            public Token paren = _paren;
            public List<Expr> arguments = _arguments;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitCallExpr(this);
            }
        }

        public class Get(Expr _object, Token _name) : Expr
        {
            public Expr _object = _object;
            public Token name = _name;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGetExpr(this);
            }
        }
        public class Set(Expr _object, Token _name, Expr _value) : Expr
        {
            public Expr _object = _object;
            public Token name = _name;
            public Expr value = _value;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSetExpr(this);
            }
        }
        public class This(Token _keyword) : Expr
        {
            public Token keyword = _keyword;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitThisExpr(this);
            }
        }
    }
}
