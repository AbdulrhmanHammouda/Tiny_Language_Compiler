
---

## 💡 Tiny Language Features

- **Data Types**: `int`, `float`, `string`
- **Identifiers**: Must start with a letter, followed by letters/digits
- **Numbers**: Integers and floating-point (e.g., `123`, `3.14`)
- **Strings**: Text enclosed in double quotes (`"Hello World"`)
- **Operators**: `+`, `-`, `*`, `/`, `:=`, `<`, `>`, `=`, `<>`, `&&`, `||`
- **Control Keywords**: `if`, `elseif`, `else`, `then`, `repeat`, `until`, `return`
- **I/O**: `read`, `write`, `endl`
- **Comments**: `/* ... */`
- **Delimiters**: `()`, `{}`, `[]`, `,`, `;`

---

## ⚙️ How It Works

1. **Lexical Analysis**  
   The lexer uses the defined regex patterns and DFAs to scan source code and break it down into valid tokens.

2. **Syntax Analysis**  
   The parser checks if the sequence of tokens adheres to the CFG, ensuring the code is syntactically valid according to the Tiny Language rules.

---

## 🚀 Getting Started

This repository focuses on the **design**.  
You can adapt it to any programming language or compiler toolchain:
- **Lexer:** Implement using tools like Lex/Flex or custom regex libraries.
- **Parser:** Implement using Yacc/Bison or recursive descent parsers.

---

## ✅ Status

- ✔️ **Phase 1**: Lexical Analyzer Design — Completed
- ✔️ **Phase 2**: Parser CFG Design — Completed
