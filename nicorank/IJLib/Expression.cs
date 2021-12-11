// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Text;

namespace IJLib
{
    namespace Expression
    {
        /// <summary>
        /// 文字列で書かれた式を評価するためのクラス
        /// </summary>
        public class Expression
        {
            /// <summary>
            /// 文字列で書かれた式を評価する
            /// </summary>
            /// <param name="expression">式</param>
            /// <returns>値</returns>
            public static Value Evaluate(string expression)
            {
                Tuple t = ParseExpression(Lex.LexicalAnalyze(expression));
                return t.Evaluate();
            }

            protected static Tuple ParseExpression(Queue<Token> queue)
            {
                Tuple t = ParsePlusMinus(queue);
                return t;
            }

            private static Tuple ParsePlusMinus(Queue<Token> queue)
            {
                List<Tuple> tuple_list = new List<Tuple>();
                List<Token> token_list = new List<Token>();
                tuple_list.Add(ParseMulDivRem(queue));
                while (queue.Count > 0)
                {
                    Token tok = queue.Peek();
                    token_list.Add(tok);
                    if (tok.IsPlus() || tok.IsMinus())
                    {
                        queue.Dequeue();
                        tuple_list.Add(ParseMulDivRem(queue));
                    }
                    else
                    {
                        break;
                    }
                }
                Tuple t = tuple_list[0];
                for (int i = 0; i < tuple_list.Count - 1; ++i)
                {
                    t = new Tuple(token_list[i], t, tuple_list[i + 1]);
                }
                return t;
            }

            private static Tuple ParseMulDivRem(Queue<Token> queue)
            {
                List<Tuple> tuple_list = new List<Tuple>();
                List<Token> token_list = new List<Token>();
                tuple_list.Add(ParseSingleOp(queue));
                while (queue.Count > 0)
                {
                    Token tok = queue.Peek();
                    token_list.Add(tok);
                    if (tok.IsMul() || tok.IsDiv() || tok.IsRem())
                    {
                        queue.Dequeue();
                        tuple_list.Add(ParseSingleOp(queue));
                    }
                    else
                    {
                        break;
                    }
                }
                Tuple t = tuple_list[0];
                for (int i = 0; i < tuple_list.Count - 1; ++i)
                {
                    t = new Tuple(token_list[i], t, tuple_list[i + 1]);
                }
                return t;
            }

            private static Tuple ParseSingleOp(Queue<Token> queue)
            {
                Token tok = queue.Dequeue();
                if (tok.IsPlus() || tok.IsMinus()) // 単項 + -
                {
                    return new Tuple(tok, ParseSingleOp(queue), null);
                }
                else if (tok.IsLeftBlacket())
                {
                    Tuple t = ParseExpression(queue);
                    Token tok2 = queue.Dequeue();
                    if (!tok2.IsRightBlacket())
                    {
                        throw new FormatException("括弧の対応がとれていません。");
                    }
                    return t;
                }
                else if (tok.kind == Token.Kind.Identifier)
                {
                    if (queue.Count > 0)
                    {
                        Token tok2 = queue.Peek();
                        if (tok2.IsLeftBlacket()) // 関数だった場合
                        {
                            queue.Dequeue();
                            List<Tuple> tuple_list = new List<Tuple>();
                            if (!queue.Peek().IsRightBlacket())
                            {
                                tuple_list.Add(ParseExpression(queue));

                                while (queue.Count > 0)
                                {
                                    Token tok3 = queue.Dequeue();
                                    if (tok3.IsComma())
                                    {
                                        tuple_list.Add(ParseExpression(queue));
                                    }
                                    else if (!tok3.IsRightBlacket())
                                    {
                                        throw new FormatException("括弧の対応がとれていません。");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            return new Tuple(new Token(Token.Kind.Function), new Tuple(tok), null, tuple_list);
                        }
                    }
                    return new Tuple(tok);
                }
                else if (tok.kind == Token.Kind.Value)
                {
                    return new Tuple(tok);
                }
                throw new FormatException("構文が正しくありません。");
            }
        }

        /// <summary>
        /// 必要に応じて int または double として評価される「値」を表すクラス
        /// </summary>
        public class Value
        {
            public enum Kind { IntValue, DoubleValue };
            public Kind kind;

            public int int_value;
            public double double_value;

            public Value(Kind k)
            {
                kind = k;
            }

            public Value(string str)
            {
                if (str.IndexOf('.') >= 0)
                {
                    kind = Kind.DoubleValue;
                    double_value = double.Parse(str);
                }
                else
                {
                    kind = Kind.IntValue;
                    int_value = int.Parse(str);
                }
            }

            public double GetDouble()
            {
                if (kind == Kind.IntValue)
                {
                    return (double)int_value;
                }
                else
                {
                    return double_value;
                }
            }

            // 小数は切り捨てて取得
            public int GetInt()
            {
                if (kind == Kind.IntValue)
                {
                    return int_value;
                }
                else
                {
                    return (int)double_value;
                }
            }

            public override string ToString()
            {
                if (kind == Kind.IntValue)
                {
                    return int_value.ToString();
                }
                else
                {
                    return double_value.ToString();
                }
            }

            public static Value operator +(Value v1, Value v2)
            {
                Value v;
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    v = new Value(Kind.IntValue);
                    v.int_value = v1.int_value + v2.int_value;
                }
                else
                {
                    v = new Value(Kind.DoubleValue);
                    v.double_value = v1.GetDouble() + v2.GetDouble();
                }
                return v;
            }

            public static Value operator -(Value v1, Value v2)
            {
                Value v;
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    v = new Value(Kind.IntValue);
                    v.int_value = v1.int_value - v2.int_value;
                }
                else
                {
                    v = new Value(Kind.DoubleValue);
                    v.double_value = v1.GetDouble() - v2.GetDouble();
                }
                return v;
            }

            public static Value operator -(Value v)
            {
                Value ret;
                if (v.kind == Kind.IntValue)
                {
                    ret = new Value(Kind.IntValue);
                    ret.int_value = -v.int_value;
                }
                else
                {
                    ret = new Value(Kind.DoubleValue);
                    ret.double_value = -v.double_value;
                }
                return ret;
            }

            public static Value operator *(Value v1, Value v2)
            {
                Value v;
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    v = new Value(Kind.IntValue);
                    v.int_value = v1.int_value * v2.int_value;
                }
                else
                {
                    v = new Value(Kind.DoubleValue);
                    v.double_value = v1.GetDouble() * v2.GetDouble();
                }
                return v;
            }

            public static Value operator /(Value v1, Value v2)
            {
                Value v;
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    v = new Value(Kind.IntValue);
                    v.int_value = v1.int_value / v2.int_value;
                }
                else
                {
                    v = new Value(Kind.DoubleValue);
                    v.double_value = v1.GetDouble() / v2.GetDouble();
                }
                return v;
            }

            public static Value operator %(Value v1, Value v2)
            {
                Value v;
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    v = new Value(Kind.IntValue);
                    v.int_value = v1.int_value % v2.int_value;
                }
                else
                {
                    v = new Value(Kind.DoubleValue);
                    v.double_value = v1.GetDouble() % v2.GetDouble();
                }
                return v;
            }

            public static bool operator < (Value v1, Value v2)
            {
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    return v1.int_value < v2.int_value;
                }
                else
                {
                    return v1.GetDouble() < v2.GetDouble();
                }
            }

            public static bool operator >(Value v1, Value v2)
            {
                if (v1.kind == Kind.IntValue && v2.kind == Kind.IntValue)
                {
                    return v1.int_value > v2.int_value;
                }
                else
                {
                    return v1.GetDouble() > v2.GetDouble();
                }
            }
        }

        public class Tuple
        {
            public Token op_token;
            public Tuple tuple1;
            public Tuple tuple2;
            public List<Tuple> tuple_list; // Token が list のときにしか使わない

            public Tuple(Token tok)
            {
                op_token = tok;
            }

            public Tuple(Token op, Tuple t1, Tuple t2)
            {
                op_token = op;
                tuple1 = t1;
                tuple2 = t2;
            }

            public Tuple(Token op, Tuple t1, Tuple t2, List<Tuple> t_list)
            {
                op_token = op;
                tuple1 = t1;
                tuple2 = t2;
                tuple_list = t_list;
            }

            public Value Evaluate()
            {
                if (op_token.IsPlus())
                {
                    Value v1 = tuple1.Evaluate();
                    if (tuple2 != null)
                    {
                        Value v2 = tuple2.Evaluate();
                        return v1 + v2;
                    }
                    else
                    {
                        return v1;
                    }
                }
                else if (op_token.IsMinus())
                {
                    Value v1 = tuple1.Evaluate();
                    if (tuple2 != null)
                    {
                        Value v2 = tuple2.Evaluate();
                        return v1 - v2;
                    }
                    else
                    {
                        return -v1;
                    }
                }
                else if (op_token.IsMul())
                {
                    return tuple1.Evaluate() * tuple2.Evaluate();
                }
                else if (op_token.IsDiv())
                {
                    return tuple1.Evaluate() / tuple2.Evaluate();
                }
                else if (op_token.IsRem())
                {
                    return tuple1.Evaluate() % tuple2.Evaluate();
                }
                else if (op_token.kind == Token.Kind.Value)
                {
                    return op_token.value;
                }
                else if (op_token.kind == Token.Kind.Function)
                {
                    switch (tuple1.op_token.str) // 関数名
                    {
                        case "int":
                            Value v = new Value(Value.Kind.IntValue);
                            v.int_value = tuple_list[0].Evaluate().GetInt();
                            return v;
                        case "abs":
                            Value v2 = tuple_list[0].Evaluate();
                            if (v2.GetDouble() < 0)
                            {
                                if (v2.kind == Value.Kind.IntValue)
                                {
                                    v2.int_value = -v2.int_value;
                                }
                                else
                                {
                                    v2.double_value = -v2.double_value;
                                }
                            }
                            return v2;
                        case "max":
                            Value max_value = tuple_list[0].Evaluate();
                            for (int i = 1; i < tuple_list.Count; ++i)
                            {
                                Value va = tuple_list[i].Evaluate();
                                if (max_value < va)
                                {
                                    max_value = va;
                                }
                            }
                            return max_value;
                        case "min":
                            Value min_value = tuple_list[0].Evaluate();
                            for (int i = 1; i < tuple_list.Count; ++i)
                            {
                                Value va = tuple_list[i].Evaluate();
                                if (va < min_value)
                                {
                                    min_value = va;
                                }
                            }
                            return min_value;
                        default:
                            throw new FormatException("関数 '" + tuple1.op_token.str + "' は存在しません。");
                    }
                }
                else
                {
                    throw new FormatException("構文解析に失敗しました。");
                }
            }
        }

        public class Token
        {
            public enum Kind { Value, Identifier, Symbol, Function, List };

            public Kind kind;

            public Value value;
            public string str;

            public Token()
            {

            }

            public Token(Kind k)
            {
                kind = k;
            }

            public override string ToString()
            {
                string ret = kind.ToString() + ": ";

                switch (kind)
                {
                    case Kind.Value:
                        ret += value.ToString();
                        break;
                    case Kind.Symbol:
                    case Kind.Identifier:
                        ret += str;
                        break;
                }
                return ret;
            }

            public bool IsComma()
            {
                return kind == Kind.Symbol && str == ",";
            }

            public bool IsPlus()
            {
                return kind == Kind.Symbol && str == "+";
            }

            public bool IsMinus()
            {
                return kind == Kind.Symbol && str == "-";
            }

            public bool IsMul()
            {
                return kind == Kind.Symbol && str == "*";
            }

            public bool IsDiv()
            {
                return kind == Kind.Symbol && str == "/";
            }

            public bool IsRem()
            {
                return kind == Kind.Symbol && str == "%";
            }

            public bool IsLeftBlacket()
            {
                return kind == Kind.Symbol && str == "(";
            }

            public bool IsRightBlacket()
            {
                return kind == Kind.Symbol && str == ")";
            }
        }

        class Lex
        {
            private enum Mode { None, Val, Ident, Symbol };

            public static string Test(string str)
            {
                string[] expression = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                string ret = "";
                for (int i = 0; i < expression.Length; ++i)
                {
                    ret += Print(LexicalAnalyze(expression[i]));
                    ret += "\r\n";
                }
                return ret;
            }

            public static string Print(Queue<Token> queue)
            {
                StringBuilder buff = new StringBuilder();
                foreach (Token tok in queue)
                {
                    buff.Append(tok.ToString());
                    buff.Append(", ");
                }
                return buff.ToString();
            }

            public static Queue<Token> LexicalAnalyze(string str)
            {
                Queue<Token> queue = new Queue<Token>();
                string val = "";
                Mode mode = Mode.None;

                for (int i = 0; i < str.Length; ++i)
                {
                    if ('0' <= str[i] && str[i] <= '9' || str[i] == '.')
                    {
                        if (mode == Mode.Symbol)
                        {
                            queue.Enqueue(SymbolToToken(val));
                            val = "";
                        }
                        if (mode != Mode.Ident)
                        {
                            mode = Mode.Val;
                        }
                        val += str[i];
                    }
                    else if ('a' <= str[i] && str[i] <= 'z' || 'A' <= str[i] && str[i] <= 'Z')
                    {
                        if (mode == Mode.Symbol)
                        {
                            queue.Enqueue(SymbolToToken(val));
                            val = "";
                        }
                        else if (mode == Mode.Val)
                        {
                            queue.Enqueue(ValToToken(val));
                            val = "";
                        }
                        mode = Mode.Ident;
                        val += str[i];
                    }
                    else if (IsSymbol(str[i]))
                    {
                        if (mode == Mode.Val)
                        {
                            queue.Enqueue(ValToToken(val));
                            val = "";
                        }
                        else if (mode == Mode.Ident)
                        {
                            queue.Enqueue(IdentifierToToken(val));
                            val = "";
                        }
                        else if (mode == Mode.Symbol)
                        {
                            queue.Enqueue(SymbolToToken(val));
                            val = "";
                        }
                        val += str[i];
                        mode = Mode.Symbol;
                    }
                    else if (IsWhiteSpace(str[i]))
                    {
                        EnqueueToken(queue, mode, val);
                        val = "";
                        mode = Mode.None;
                    }
                }
                EnqueueToken(queue, mode, val);
                return queue;
            }

            private static bool IsSymbol(char c)
            {
                char[] sym = { '+', '-', '*', '/', '%', '(', ')', ',' };

                for (int i = 0; i < sym.Length; ++i)
                {
                    if (c == sym[i])
                    {
                        return true;
                    }
                }
                return false;
            }

            private static bool IsWhiteSpace(char c)
            {
                char[] space = { ' ', '\t', '\r', '\n' };

                for (int i = 0; i < space.Length; ++i)
                {
                    if (c == space[i])
                    {
                        return true;
                    }
                }
                return false;
            }

            private static void EnqueueToken(Queue<Token> queue, Mode mode, string val)
            {
                if (mode == Mode.Val)
                {
                    queue.Enqueue(ValToToken(val));
                }
                else if (mode == Mode.Ident)
                {
                    queue.Enqueue(IdentifierToToken(val));
                }
                else if (mode == Mode.Symbol)
                {
                    queue.Enqueue(SymbolToToken(val));
                }
            }

            private static Token ValToToken(string str)
            {
                Token token = new Token();
                token.kind = Token.Kind.Value;
                token.value = new Value(str);
                return token;
            }

            private static Token IdentifierToToken(string str)
            {
                Token token = new Token();
                token.kind = Token.Kind.Identifier;
                token.str = str;
                return token;
            }

            private static Token SymbolToToken(string str)
            {
                Token token = new Token();
                token.kind = Token.Kind.Symbol;
                token.str = str;
                return token;
            }
        }

        /// <summary>
        /// Expression のテストのためのクラス
        /// </summary>
        public class TestExpression : Expression
        {
            public static string MakeExpression(string str)
            {
                string[] expression = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Random r = new Random();
                string[] op = { "+", "-", "*", "/", "%" };
                string ret = "";
                string answer = "";
                int blacket = 0;
                for (int i = 0; i < 10; ++i)
                {
                    int n = r.Next(100) + 1;
                    string ex = "";
                    for (int j = 0; j < n; ++j)
                    {
                        ex += (r.NextDouble() * r.Next()).ToString();
                        if (blacket > 0 && r.Next(100) < 20)
                        {
                            ex += ")";
                            --blacket;
                        }
                        if (j < n - 1)
                        {
                            ex += op[r.Next(op.Length)];
                            if (r.Next(100) < 20)
                            {
                                ex += "(";
                                ++blacket;
                            }
                        }
                    }
                    while (blacket > 0)
                    {
                        ex += ")";
                        --blacket;
                    }
                    for (int j = 0; j < 1; ++j)
                    {
                        Tuple t = ParseExpression(Lex.LexicalAnalyze(ex));
                        answer += t.Evaluate().ToString();
                        answer += "\r\n";
                    }
                    ret += ex + ",\r\n";
                }
                ret += answer;
                return ret;
            }

            public static string Test(string str)
            {
                string[] expression = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                string answer = "";
                for (int j = 0; j < expression.Length; ++j)
                {
                    Tuple t = ParseExpression(Lex.LexicalAnalyze(expression[j]));
                    answer += t.Evaluate().ToString();
                    answer += "\r\n";
                }
                return answer;
            }

            public static string Test2(string str)
            {
                double[] v = { 1.0 };
                string ret = "";
                for (int i = 0; i < v.Length; ++i)
                {
                    ret += v[i].ToString() + "\r\n";
                }
                return ret;
            }
        }
    }
}
