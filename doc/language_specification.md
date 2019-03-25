# KJU language specification

## Evaluation

- eager evaluation

## Semantics

### Comments

- nested `/*  */` comments

    ```
    /* some comments */

    /*
        var a = 4; /* nested comment example */
    */
    ```

### Identifiers

- differentiate by first letter case

- types

    - `[A-Z][a-zA-Z0-9_]*`

    - convention: PascalCase

- variables

    - `[a-z][a-zA-Z0-9_]*`

    - convention: camelCase

- functions

    - `[a-z][a-zA-Z0-9_]*`

    - convention: camelCase

### Types

- Unit

    - represents variable without information

- Bool

    - possible values: `true`, `false`

    - comparison only by `==` or `!=`

- Int

    - representation by machine int (eg. on amd64 signed int64)

### Blocks

- curly braces

- return Unit

- open questions:

    - maybe it should return last expression value

### Declarations

- variables

    - type on declaration not required

        ```
        var x : Int = 3;
        var y = 4;
        ```

- functions

    - no forward-declaration requirement

    - no default values supported

        eg.
        ```
        fun f (x : Int, y : Bool) : Int {
            return 3;
        }
        ```

    - overloading possible

    - local function definitions:

        - overrides only function with the same set of arguments

        - does not impact usages before definition

        eg.
        ```
        fun f (x : Int)  : Bool {...}   # A
        fun f (x : Bool) : Bool {...}   # B

        fun g () : Unit {
            f (3);                      # executes A
            f (true);                   # executes B

            fun f(x : Int) : Unit {...} # C

            f (3);                      # executes C
            f (true);                   # executes B
        }
        ```

### Instructions

- ending with semicolon

- assignment `=`

    - returns value of right hand expression

- conditional

    - returns Unit

    - forced `else` block

    ```
    if <condition expression> then <then block> else <else block>
    ```

    eg.
    ```
    if (x == 42)
        then {y = 43;}
        else {y = 21;};

    if y == 43
        then {x = 43;}
        else {};
    ```

- `while` loop

    - `break` exits only current loop

    ```
    while <condition expression> <body block>
    ```

    eg.
    ```
    while (x < 10) {x = x + 1;}

    while b == true {b = false;}
    ```

- __all__ instructions are expressions (ie. return values, if undefined, return Unit)

- block returns Unit

- `return` statement exits current function. If return type of function is Unit, return does not accept any value. Otherwise it requires expression with type equal to return type of function.

    ````
    fun f() : Unit {
        return;
    }

    fun g() : Int {
        return 3;
    }
    ````

### Global scope

- no global variables

- entry function: `kju`

    - must exist

    - can return Unit or Int

    ```
    fun kju () : Unit {
    }

    fun kju () : Int {
        return 0;
    }
    ```

### Supported operators

- arithmetic operators: `+`, `-`, `*`, `/`, `%`, `+=`, `-=`, `*=`, `/=`, `%=`

- comparators: `<`, `<=`, `>`, `>=`, `==`, `!=`

## Basic requirements

- numerical types

- numerical operations: at least `+` `-` `*`

- some control flow: eg. ifs, loops

- at least one boolean type (with at least `==`)

- functions with arguments and at least one return value

- statically typed

- recursion supported

- local functions in functions
