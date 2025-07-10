using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            foreach (Node child in root.Children)
            {
                if (child == null) continue;
                TreeNode childNode = PrintTree(child);
                if (childNode != null)
                {
                    tree.Nodes.Add(childNode);
                }
            }
            return tree;
        }
        bool IsNextMainFunction()
        {
            if (InputPointer + 3 >= TokenStream.Count) return false;

            return TokenStream[InputPointer].token_type == Token_Class.Int &&
                   TokenStream[InputPointer + 1].token_type == Token_Class.Main &&
                   TokenStream[InputPointer + 2].token_type == Token_Class.LParanthesis &&
                   TokenStream[InputPointer + 3].token_type == Token_Class.RParanthesis;
        }

        Node Program()
        {
            Node program = new Node("Program");

            program.Children.Add(Function_Statements()); // FunctionStatements
            program.Children.Add(Main_Function());       // Main_Function

            return program;
        }
        Node Function_Statements()
{
    Node funcStatements = new Node("FunctionStatements");

    while (InputPointer < TokenStream.Count && !IsNextMainFunction())
    {
        funcStatements.Children.Add(Function_Statement());
    }

    return funcStatements;
}

        Node Function_Statements2()
        {
            Node funcStatements2 = new Node("FunctionStatements`");

            // While the next function is NOT main
            if (InputPointer < TokenStream.Count && !IsNextMainFunction())
            {
                funcStatements2.Children.Add(Function_Statement());
                funcStatements2.Children.Add(Function_Statements2());
            }
            // else ε (empty production — no node added)

            return funcStatements2;
        }



        Node Function_Statement()
        {
            Node functionStatement = new Node("FunctionStatement");
            functionStatement.Children.Add(Function_Declaration());
            functionStatement.Children.Add(Function_Body());
            return functionStatement;
        }

        Node Main_Function()
        {
            Node mainFunction = new Node("MainFunction");

            // Check for the 'int' token
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Int)
            {
                mainFunction.Children.Add(match(Token_Class.Int));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'int' before main function declaration.");
                return mainFunction;  // Return early in case of error
            }

            // Check for the 'main' keyword
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Main)
            {
                mainFunction.Children.Add(match(Token_Class.Main));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'main' keyword after 'int'.");
                return mainFunction;
            }

            // Check for the opening parenthesis '('
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                mainFunction.Children.Add(match(Token_Class.LParanthesis));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected '(' after 'main'.");
                return mainFunction;
            }

            // Check for the closing parenthesis ')'
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.RParanthesis)
            {
                mainFunction.Children.Add(match(Token_Class.RParanthesis));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected ')' after 'main' parameter list.");
                return mainFunction;
            }

            // Check for the opening curly brace '{' to start the function body
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LCurlyBraces)
            {
                mainFunction.Children.Add(Function_Body());
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected '{' at the start of the main function body.");
            }

            return mainFunction;
        }


        Node Function_Declaration()
        {
            Node functionDecl = new Node("FunctionDeclaration");
            functionDecl.Children.Add(match(Token_Class.Int));
            functionDecl.Children.Add(match(Token_Class.Identifier));
            functionDecl.Children.Add(match(Token_Class.LParanthesis));
            functionDecl.Children.Add(Parameter_List());
            functionDecl.Children.Add(match(Token_Class.RParanthesis));
            return functionDecl;
        }

        Node Parameter_List()
        {
            Node paramList = new Node("ParameterList");

            paramList.Children.Add(Parameters());  // Parameters → Parameter Parameters'

            return paramList;
        }

        Node Parameters()
        {
            Node parameters = new Node("Parameters");

            parameters.Children.Add(Parameter());       // Parameter
            parameters.Children.Add(Parameters_());     // Parameters'

            return parameters;
        }
        Node Parameters_()
        {
            Node parameters_ = new Node("Parameters'");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters_.Children.Add(match(Token_Class.Comma));   // ,
                parameters_.Children.Add(Parameter());                 // Parameter
                parameters_.Children.Add(Parameters_());               // Parameters'
            }
            // else ε

            return parameters_;
        }
        Node Parameter()
        {
            Node parameter = new Node("Parameter");

            parameter.Children.Add(Datatype());                      // Datatype
            parameter.Children.Add(match(Token_Class.Identifier));   // id

            return parameter;
        }

        Node Function_Body()
        {
            Node functionBody = new Node("Function_Body");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LCurlyBraces)
            {
                functionBody.Children.Add(match(Token_Class.LCurlyBraces));

                functionBody.Children.Add(Statements());

                // Check for optional return statement
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Return)
                {
                    functionBody.Children.Add(Return_Statement());
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Missing return statement at the end of the function body.");
                }

                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.RCurlyBraces)
                {
                    functionBody.Children.Add(match(Token_Class.RCurlyBraces));
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Missing '}' at the end of the function body.");
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected '{' at the start of the function body.");
            }

            return functionBody;
        }


        Node Statement()
        {
            Node statement = new Node("Statement");

            if (InputPointer >= TokenStream.Count)
            {
                Errors.Error_List.Add("Error: Unexpected end of input while parsing statement.");
                return statement;
            }

            switch (TokenStream[InputPointer].token_type)
            {
                case Token_Class.Write:
                    statement.Children.Add(Write_Statement());
                    break;
                case Token_Class.Read:
                    statement.Children.Add(Read_Statement());
                    break;
                case Token_Class.String:
                case Token_Class.Float:
                case Token_Class.Int:
                    statement.Children.Add(Declaration_Statement());
                    break;
                case Token_Class.Return:
                    statement.Children.Add(Return_Statement());
                    break;
                case Token_Class.If:
                    statement.Children.Add(If_Statement());
                    break;
                case Token_Class.Repeat:
                    statement.Children.Add(Repeat_Statement());
                    break;
                case Token_Class.Comment:
                    statement.Children.Add(Comment_Statement());
                    break;
                default:
                    statement.Children.Add(Assignment_Statement());
                    break;
            }

            return statement;
        }
        Node Declaration_Statement()
        {
            Node declaration = new Node("Declaration_Statement");
            declaration.Children.Add(Datatype());               // Datatype
            declaration.Children.Add(IdentifierList());         // IdentifierList
            declaration.Children.Add(match(Token_Class.Semicolon)); // ;
            return declaration;
        }
        Node Datatype()
        {
            Node datatype = new Node("Datatype");

            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Int ||
                 TokenStream[InputPointer].token_type == Token_Class.Float ||
                 TokenStream[InputPointer].token_type == Token_Class.String))
            {
                datatype.Children.Add(match(TokenStream[InputPointer].token_type));
            }

            return datatype;
        }
        Node IdentifierList()
        {
            Node list = new Node("IdentifierList");
            list.Children.Add(IdentifierAssignment());
            list.Children.Add(IdentifierList_());
            return list;
        }

        Node IdentifierList_()
        {
            Node listPrime = new Node("IdentifierList'");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                listPrime.Children.Add(match(Token_Class.Comma));
                listPrime.Children.Add(IdentifierAssignment());
                listPrime.Children.Add(IdentifierList_());
            }
            // ε → do nothing (empty node)
            return listPrime;
        }
        Node IdentifierAssignment()
        {
            Node assign = new Node("IdentifierAssignment");
            assign.Children.Add(match(Token_Class.Identifier));
            assign.Children.Add(Declaration_Init());
            return assign;
        }

        Node Declaration_Init()
        {
            Node init = new Node("Declaration_Init");

            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                init.Children.Add(match(Token_Class.AssignOp)); // :=
                init.Children.Add(Expression());                    // Expression
            }
            // ε → do nothing (empty node)
            return init;
        }
        Node Comment_Statement()
        {
            Node comment = new Node("Comment_Statement");
            comment.Children.Add(match(Token_Class.Comment));
            return comment;
        }

        Node Return_Statement()
        {
            Node returnStatement = new Node("ReturnStatement");

            // Match 'return'
            match(Token_Class.Return);
            returnStatement.Children.Add(new Node("return"));

            // Parse Expression
            returnStatement.Children.Add(Expression());

            // Match ';'
            match(Token_Class.Semicolon);
            returnStatement.Children.Add(new Node(";"));

            return returnStatement;
        }


        Node Repeat_Statement()
        {
            Node repeatStmt = new Node("Repeat_Statement");
            repeatStmt.Children.Add(match(Token_Class.Repeat));
            repeatStmt.Children.Add(Statements());                  // Allow multiple statements
            repeatStmt.Children.Add(match(Token_Class.Until));
            repeatStmt.Children.Add(Condition_Statement());
            return repeatStmt;
        }
        Node Statements()
        {
            Node statements = new Node("Statements");

            bool hasStatements = false;

            while (InputPointer < TokenStream.Count && IsStartOfStatement(TokenStream[InputPointer].token_type))
            {
                Node stmtNode = Statement();

                if (stmtNode != null)
                {
                    statements.Children.Add(stmtNode);
                    hasStatements = true;
                }
                else
                {
                    string errorMsg = $"Parsing Error: Invalid or unexpected token '{TokenStream[InputPointer].token_type}' at position {InputPointer}.";
                    Errors.Error_List.Add(errorMsg);
                }
            }

            return statements;
        }


        // Helper to detect start of a statement
        bool IsStartOfStatement(Token_Class tokenType)
        {
            return tokenType == Token_Class.Read ||
                   tokenType == Token_Class.Write ||
                   tokenType == Token_Class.Identifier ||
                   tokenType == Token_Class.Int ||
                   tokenType == Token_Class.Float ||
                   tokenType == Token_Class.String ||  // Declarations
                   tokenType == Token_Class.If ||
                   tokenType == Token_Class.Repeat ||
                   tokenType == Token_Class.Comment;
        }



        Node Condition_Statement()
        {
            Node condStmt = new Node("ConditionStatement");
            condStmt.Children.Add(Boolean_Or());
            return condStmt;
        }

        Node Boolean_Or()
        {
            Node boolOr = new Node("Boolean_Or");
            boolOr.Children.Add(Boolean_And());
            boolOr.Children.Add(Boolean_OrPrime());
            return boolOr;
        }

        Node Boolean_OrPrime()
        {
            Node boolOrPrime = new Node("Boolean_Or'");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                boolOrPrime.Children.Add(match(Token_Class.OrOp)); // ||
                boolOrPrime.Children.Add(Boolean_And());
                boolOrPrime.Children.Add(Boolean_OrPrime()); // recursive call
            }
            // ε (empty) case: do nothing
            return boolOrPrime;
        }

        Node Boolean_And()
        {
            Node boolAnd = new Node("Boolean_And");
            boolAnd.Children.Add(Condition());
            boolAnd.Children.Add(Boolean_AndPrime());
            return boolAnd;
        }

        Node Boolean_AndPrime()
        {
            Node boolAndPrime = new Node("Boolean_And'");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AndOp)
            {
                boolAndPrime.Children.Add(match(Token_Class.AndOp)); // &&
                boolAndPrime.Children.Add(Condition());
                boolAndPrime.Children.Add(Boolean_AndPrime()); // recursive call
            }
            // ε (empty) case: do nothing
            return boolAndPrime;
        }

        Node Condition()
        {
            Node cond = new Node("Condition");
            cond.Children.Add(match(Token_Class.Identifier)); // id
            cond.Children.Add(Condition_Operator());
            cond.Children.Add(Term()); // reused from your existing parser
            return cond;
        }

        Node Condition_Operator()
        {
            Node op = new Node("Condition_Operator");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class t = TokenStream[InputPointer].token_type;
                if (t == Token_Class.LessThanOp || t == Token_Class.GreaterThanOp ||
                    t == Token_Class.EqualOp || t == Token_Class.NotEqualOp)
                {
                    op.Children.Add(match(t));
                }
                else
                {
                    op.Children.Add(new Node("Error: Invalid condition operator"));
                }
            }
            return op;
        }


        Node If_Statement()
        {
            Node ifStmt = new Node("IfStatement");

            // Ensure the 'if' keyword is present
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.If)
            {
                ifStmt.Children.Add(match(Token_Class.If));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'if' at the start of the if statement.");
                return ifStmt; // Return early in case of error
            }

            // Parse the condition in the if statement
            ifStmt.Children.Add(Condition_Statement());

            // Ensure the 'then' keyword is present after the condition
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Then)
            {
                ifStmt.Children.Add(match(Token_Class.Then));
                ifStmt.Children.Add(Statements());
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Missing 'then' in if statement after condition.");
                return ifStmt; // Return early if 'then' is missing
            }

            // Check for an 'elseif' clause
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Elseif)
            {
                ifStmt.Children.Add(ElseIf_Statement());
            }

            // Check for an 'else' clause
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                ifStmt.Children.Add(Else_Statement());
            }

            // Ensure the 'end' keyword is present to close the if statement
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.End)
            {
                ifStmt.Children.Add(match(Token_Class.End));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Missing 'end' to close if statement.");
            }

            // Check for unexpected tokens after the 'end'
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.End)
            {
                Errors.Error_List.Add("Parsing Error: Unexpected token after 'end'. Ensure the if statement is properly closed.");
            }

            return ifStmt;
        }


        Node ElseIf_Statement()
        {
            Node elseifStmt = new Node("ElseIfStatement");

            // Ensure the 'elseif' keyword is present
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Elseif)
            {
                elseifStmt.Children.Add(match(Token_Class.Elseif));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'elseif' in else-if statement.");
                return elseifStmt; // Return early in case of error
            }

            
                elseifStmt.Children.Add(Condition_Statement());

            // Ensure the 'then' keyword is present after the condition
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Then)
            {
                elseifStmt.Children.Add(match(Token_Class.Then));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Missing 'then' after condition in elseif statement.");
                return elseifStmt; // Return early if 'then' is missing
            }

            
            elseifStmt.Children.Add(Statements());
            

            return elseifStmt;
        }

        Node Else_Statement()
        {
            Node elseStmt = new Node("ElseStatement");

            // Ensure the 'else' keyword is present
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                elseStmt.Children.Add(match(Token_Class.Else));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected 'else' in else statement.");
                return elseStmt; // Return early if 'else' is missing
            }

            
            elseStmt.Children.Add(Statements());
            
            return elseStmt;
        }

        Node Read_Statement()
        {
            Node readStmt = new Node("ReadStatement");
            readStmt.Children.Add(match(Token_Class.Read));
            readStmt.Children.Add(match(Token_Class.Identifier));
            readStmt.Children.Add(match(Token_Class.Semicolon));
            return readStmt;
        }

        Node Write_Statement()
        {
            Node writeStmt = new Node("WriteStatement");

            // Match "write"
            writeStmt.Children.Add(match(Token_Class.Write));

            // Match either Expression or "endl"
            if (TokenStream[InputPointer].token_type == Token_Class.Endl)
            {
                writeStmt.Children.Add(match(Token_Class.Endl));
            }
            else
            {
                writeStmt.Children.Add(Expression());
            }

            // Match ";"
            writeStmt.Children.Add(match(Token_Class.Semicolon));

            return writeStmt;
        }


        Node Assignment_Statement()
        {
            Node assignStmt = new Node("AssignmentStatement");
            assignStmt.Children.Add(match(Token_Class.Identifier));
            assignStmt.Children.Add(match(Token_Class.AssignOp));
            assignStmt.Children.Add(Expression());
            assignStmt.Children.Add(match(Token_Class.Semicolon));
            return assignStmt;
        }

        Node Expression()
        {
            Node expr = new Node("Expression");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class type = TokenStream[InputPointer].token_type;
                if (type == Token_Class.StringLiteral)
                {
                    expr.Children.Add(match(Token_Class.StringLiteral));
                }
                else if (type == Token_Class.Constant || type == Token_Class.Identifier || type == Token_Class.LParanthesis)
                {
                    expr.Children.Add(Equation());
                }
                else
                {
                    expr.Children.Add(new Node("Error: Invalid expression"));
                }
            }
            return expr;
        }
        Node Equation()
        {
            Node equation = new Node("Equation");
            equation.Children.Add(EqTerm());
            while (InputPointer < TokenStream.Count &&
                  (TokenStream[InputPointer].token_type == Token_Class.PlusOp ||
                   TokenStream[InputPointer].token_type == Token_Class.MinusOp))
            {
                equation.Children.Add(match(TokenStream[InputPointer].token_type)); // addop
                equation.Children.Add(EqTerm());
            }
            return equation;
        }

        Node EqTerm()
        {
            Node term = new Node("EqTerm");
            term.Children.Add(Factor());
            while (InputPointer < TokenStream.Count &&
                  (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp ||
                   TokenStream[InputPointer].token_type == Token_Class.DivideOp))
            {
                term.Children.Add(match(TokenStream[InputPointer].token_type)); // mulop
                term.Children.Add(Factor());
            }
            return term;
        }

        Node Factor()
        {
            Node factor = new Node("Factor");
            if (TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                factor.Children.Add(match(Token_Class.LParanthesis));
                factor.Children.Add(Equation());
                factor.Children.Add(match(Token_Class.RParanthesis));
            }
            else
            {
                factor.Children.Add(Term());
            }
            return factor;
        }

        Node Term()
        {
            Node term = new Node("Term");
            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                {
                    term.Children.Add(Function_Call());
                }
                else
                {
                    term.Children.Add(match(Token_Class.Identifier));
                }
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Constant)
            {
                term.Children.Add(match(Token_Class.Constant));
            }
            return term;
        }
        Node Function_Call()
        {
            Node call = new Node("FunctionCall");
            call.Children.Add(match(Token_Class.Identifier));
            call.Children.Add(match(Token_Class.LParanthesis));
            call.Children.Add(Argument_List());
            call.Children.Add(match(Token_Class.RParanthesis));
            return call;
        }

        Node Argument_List()
        {
            Node argList = new Node("ArgumentList");

            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Identifier ||
                 TokenStream[InputPointer].token_type == Token_Class.Constant))
            {
                argList.Children.Add(Arguments());
            }

            // Else ε (do nothing)
            return argList;
        }
        Node Arguments()
        {
            Node arguments = new Node("Arguments");

            arguments.Children.Add(Argument());    // Argument
            arguments.Children.Add(Arguments_());  // Arguments'

            return arguments;
        }
        Node Arguments_()
        {
            Node argumentsDash = new Node("Arguments'");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                argumentsDash.Children.Add(match(Token_Class.Comma));
                argumentsDash.Children.Add(Argument());
                argumentsDash.Children.Add(Arguments_()); // recursive
            }

            // Else ε (do nothing)
            return argumentsDash;
        }


        Node Argument()
        {
            Node arg = new Node("Argument");
            arg.Children.Add(match(TokenStream[InputPointer].token_type)); // id or num
            return arg;
        }


        Node match(Token_Class ExpectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if (TokenStream[InputPointer].token_type == ExpectedToken)
                {
                    Node newNode = new Node(TokenStream[InputPointer].lex);
                    InputPointer++;
                    return newNode;
                }
                InputPointer++;
                return new Node("Error: Expected " + ExpectedToken);
            }
            return new Node("Error");
        }
    }
}