
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

## 🚀 Usage & Integration

1. **Lexical Analyzer**  
   - Translate regex and DFA designs into your tool of choice (e.g., Flex/Lex or custom code).

2. **Parser**  
   - Use CFG with Yacc/Bison or build a recursive‑descent parser.

3. **Combine**  
   - Feed the token stream from Phase 1 into the Phase 2 parser to validate Tiny Language programs.

---

## ✅ Status

- **Phase 1**: ✔ Design of Lexical Analyzer (regex + DFA)  
- **Phase 2**: ✔ Design of Parser (complete CFG)
