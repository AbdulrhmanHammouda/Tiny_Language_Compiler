using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum Token_Class
{
    Int, Float, String, Read, Write, Main,
    Repeat, Until, If, Elseif, Else, Then, Return, Endl, End, Dot, Comma, LParanthesis, RParanthesis,
    EqualOp, NotEqualOp, LessThanOp, GreaterThanOp, AndOp, OrOp,
    PlusOp, MinusOp, MultiplyOp, DivideOp, AssignOp, Identifier, Number, Comment, LCurlyBraces, RCurlyBraces, Constant, StringLiteral, Semicolon,
    
}

namespace JASON_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords = new Dictionary<string, Token_Class>(StringComparer.OrdinalIgnoreCase)
            {
                {"IF", Token_Class.If},
                {"END", Token_Class.End},
                {"ELSE", Token_Class.Else},
                {"READ", Token_Class.Read},
                {"THEN", Token_Class.Then},
                {"UNTIL", Token_Class.Until},
                {"WRITE", Token_Class.Write},
                {"MAIN", Token_Class.Main},
                {"REPEAT", Token_Class.Repeat},
                {"ELSEIF", Token_Class.Elseif},
                {"RETURN", Token_Class.Return},
                {"ENDL", Token_Class.Endl},
                {"STRING", Token_Class.String},
                {"INT", Token_Class.Int},
                {"FLOAT", Token_Class.Float},
            };

            Operators = new Dictionary<string, Token_Class>
            {
                {".", Token_Class.Dot},
                {";", Token_Class.Semicolon},
                {",", Token_Class.Comma},
                {"(", Token_Class.LParanthesis},
                {")", Token_Class.RParanthesis},
                {"{", Token_Class.LCurlyBraces},
                {"}", Token_Class.RCurlyBraces},
                {"=", Token_Class.EqualOp},
                {"<", Token_Class.LessThanOp},
                {">", Token_Class.GreaterThanOp},
                {"!=", Token_Class.NotEqualOp},
                {"+", Token_Class.PlusOp},
                {"-", Token_Class.MinusOp},
                {"*", Token_Class.MultiplyOp},
                {"/", Token_Class.DivideOp},
                {":=", Token_Class.AssignOp},
                {"&&", Token_Class.AndOp},
                {"||", Token_Class.OrOp},
                {"<>", Token_Class.NotEqualOp}
            };
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            int i = 0;

            while (i < SourceCode.Length)
            {
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();
                int j = i + 1;

                // Skip whitespace
                if (char.IsWhiteSpace(CurrentChar))
                {
                    i = j;
                    continue;
                }

                // Identifier or reserved word
                if (char.IsLetter(CurrentChar))
                {
                    while (j < SourceCode.Length && (isLetter(SourceCode[j]) || isDigit(SourceCode[j])))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                }
                // Number / constant (modify to include extra dots, then detect invalid token)
                else if (isDigit(CurrentChar))
                {
                    // Collect all digits and dots.
                    while (j < SourceCode.Length && (isDigit(SourceCode[j]) || SourceCode[j] == '.'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    // Count dots in the token.
                    int dotCount = 0;
                    foreach (char c in CurrentLexeme)
                    {
                        if (c == '.') dotCount++;
                    }
                    if (dotCount > 1)
                    {
                        Errors.Error_List.Add(CurrentLexeme + " <== Too many floating points");
                        i = j;
                        continue;
                    }

                    // Check for an invalid character like 'a' in "1212a"
                    if (j < SourceCode.Length && isLetter(SourceCode[j]))
                    {
                        while (j < SourceCode.Length && (isLetter(SourceCode[j]) || isDigit(SourceCode[j])))
                        {
                            CurrentLexeme += SourceCode[j];
                            j++;
                        }
                        Errors.Error_List.Add(CurrentLexeme + " <== Invalid identifier");
                        i = j;
                        continue;
                    }

                    FindTokenClass(CurrentLexeme);
                    i = j;
                    continue;
                }
                // Comments: completely ignore comments.
                else if (CurrentChar == '/' && j < SourceCode.Length && SourceCode[j] == '*')
                {
                    CurrentLexeme += SourceCode[j];
                    j++;
                    bool closed = false;
                    while (j < SourceCode.Length - 1)
                    {
                        CurrentLexeme += SourceCode[j];
                        if (SourceCode[j] == '*' && SourceCode[j + 1] == '/')
                        {
                            CurrentLexeme += "/";
                            j += 2;
                            closed = true;
                            break;
                        }
                        j++;
                    }

                    if (!closed)
                    {
                        Errors.Error_List.Add(CurrentLexeme + " <== Unclosed comment");
                        i = j;
                        continue;
                    }

                    // Completely ignore comments (do not add as a token)
                    i = j;
                    continue;
                }
                // String literal that cannot span multiple lines.
                else if (CurrentChar == '"')
                {
                    bool closed = false;
                    while (j < SourceCode.Length && SourceCode[j] != '"' && SourceCode[j] != '\n' && SourceCode[j] != '\r')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    if (j < SourceCode.Length && SourceCode[j] == '"')
                    {
                        CurrentLexeme += '"';
                        j++;
                        closed = true;
                    }

                    if (!closed)
                    {
                        // Report the error and skip the rest of the line.
                        Errors.Error_List.Add(CurrentLexeme + " <== Invalid token");
                        while (j < SourceCode.Length && SourceCode[j] != '\n')
                        {
                            j++;
                        }
                        i = j;
                        continue;
                    }

                    FindTokenClass(CurrentLexeme);
                    i = j;
                    continue;
                }
                // Multichar operators or single operators.
                else
                {
                    if (j < SourceCode.Length)
                    {
                        char NextChar = SourceCode[j];
                        string combined = CurrentChar.ToString() + NextChar.ToString();
                        if (Operators.ContainsKey(combined))
                        {
                            CurrentLexeme = combined;
                            j++;
                        }
                    }
                }

                FindTokenClass(CurrentLexeme);
                i = j;
            }

            // Assuming there is a global token stream pointer.
            JASON_Compiler.TokenStream = Tokens;
        }

        void FindTokenClass(string Lex)
        {
            if (Lex.Length == 0) return;

            Token token = new Token { lex = Lex };
            if (ReservedWords.ContainsKey(Lex.ToUpper()))
            {
                token.token_type = ReservedWords[Lex.ToUpper()];
                Tokens.Add(token);
            }
            else if (IsIdentifier(Lex))
            {
                token.token_type = Token_Class.Identifier;
                Tokens.Add(token);
            }
            else if (isConstant(Lex))
            {
                token.token_type = Token_Class.Constant;
                Tokens.Add(token);
            }
            else if (Operators.ContainsKey(Lex))
            {
                token.token_type = Operators[Lex];
                Tokens.Add(token);
            }
            else if (isStringLiteral(Lex))
            {
                token.token_type = Token_Class.StringLiteral;
                Tokens.Add(token);
            }
            else if (IsComment(Lex))
            {
                // Comments are ignored.
                return;
            }
            else
            {
                Errors.Error_List.Add(Lex + " <== Invalid token");
            }
        }

        public bool IsIdentifier(string lex)
        {
            return Regex.IsMatch(lex, @"^[a-zA-Z][a-zA-Z0-9]*$");
        }

        bool isConstant(string lex)
        {
            return Regex.IsMatch(lex, @"^\d+(\.\d+)?$");
        }

        bool isDigit(char c)
        {
            return char.IsDigit(c);
        }

        bool isLetter(char c)
        {
            return char.IsLetter(c);
        }

        bool isStringLiteral(string lex)
        {
            return lex.Length >= 2 && lex.StartsWith("\"") && lex.EndsWith("\"");
        }

        public bool IsComment(string lex)
        {
            return lex.StartsWith("/*") && lex.EndsWith("*/");
        }
    }
}
