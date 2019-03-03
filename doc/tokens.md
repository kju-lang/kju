# Tokens

These tokens are described with simple regular expressions – special constructions are limited to `*`, `|` and character classes(`[...]`) and are escaped with `\` when necessary.

## Delimiters

 - ``lbrace ::= `{` ``
 - ``rbrace ::= `}` ``
 - ``lparen ::= `(` ``
 - ``rparen ::= `)` ``
 - ``lcomment ::= `/\*` ``
 - ``rcomment ::= `\*/` ``
 - ``comma ::= `,` ``
 - ``colon ::= `:` ``
 - ``semicolon ::= `;` ``
 - ``whitespace ::= `[ \r\n\t][ \r\n\t]*` ``

## Keywords
 - ``if ::= `if` ``
 - ``then ::= `then` ``
 - ``else ::= `else` ``
 - ``while ::= `while` ``
 - ``break ::= `break` ``
 - ``continue ::= `continue` ``
 - ``var ::= `var` ``
 - ``fun ::= `fun` ``
 - ``return ::= `return` ``

## Fundamental types
 - ``int ::= `Int` ``
 - ``bool ::= `Bool` ``
 - ``unit ::= `Unit` ``

## Constants and identifiers
 - ``decimal_value ::= `0|[1-9][0-9]*` ``
 - ``true ::= `true` ``
 - ``false ::= `false` ``
 - ``type_id ::= `[A-Z][a-zA-Z0-9_]*` ``
 - ``fun_var_id ::= `[a-z][a-zA-Z0-9_]*` ``

## Operators
 - ``eq ::= `=` ``
 - ``eqeq ::= `==` ``
 - ``lt ::= `<` ``
 - ``gt ::= `>` ``
 - ``exclam ::= `!` ``
 - ``exclameq ::= `!=` ``
 - ``lteq ::= '<=` ``
 - ``gteq ::= `>=` ``
 - ``plus ::= `+` ``
 - ``minus ::= `-` ``
 - ``asterisk ::= `\*` ``
 - ``slash ::= `/` ``
 - ``percent ::= `%` ``
 - ``and ::= `&&` ``
 - ``or ::= `\|\|` ``
 - ``pluseq ::= `+=` ``
 - ``minuseq ::= `-=` ``
 - ``asteriskeq ::= `\*=` ``
 - ``slasheq ::= `/=` ``
 - ``percenteq ::= `%=` ``
