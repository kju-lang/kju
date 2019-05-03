import random, typing, copy, sys
from typing import List, Dict, Any

int_bin_ops = ['+', '-']
compare_bin_ops = ['<', '>', '<=', '>=', '==']
bool_bin_ops = ['||', '&&']

functions = {'bool': [], 'int': []} # type: Dict[str, List['Function']]
all_functions = []
variables = {'bool': [], 'int': []} # type: Dict[str, List[str]]
function_code = [] # type: List[str]
block_depth = 0
expr_depth = 0

class Function(typing.NamedTuple):
    name: str
    return_type: str
    arg_types: List[str]
    body: Any
    return_expr: str

    @classmethod
    def generate(self):
        global variables
        argc = random.randrange(4)
        variables = {'bool': [], 'int': []}
        arg_types = []
        for i in range(argc):
            kind = random.choice(['bool', 'int'])
            name = 'arg%d' % i
            variables[kind].append(name)
            arg_types.append(kind)
        return_type = random.choice(['bool', 'int'])

        body = Block.generate()
        return_value = generate(return_type)

        func = Function(
            name='fun%s%d' % (return_type, len(functions[return_type])),
            return_type=return_type,
            arg_types=arg_types,
            body=body,
            return_expr=return_value
        )
        functions[return_type].append(func)
        all_functions.append(func)
        return func

    def render_python(self):
        return 'def %s(%s):\n%s\n    return %s' % (
            self.name,
            ', '.join( 'arg%d' % i for i, arg_type in enumerate(self.arg_types) ),
            indent(self.body.render_python()),
            self.return_expr.render_python(),
        )

    def render_kju(self):
        return 'fun %s(%s): %s {\n%s\n    return %s;\n}' % (
            self.name,
            ', '.join( 'arg%d: %s' % (i, arg_type.capitalize()) for i, arg_type in enumerate(self.arg_types) ),
            self.return_type.capitalize(),
            indent(self.body.render_kju()),
            self.return_expr.render_kju(),
        )

class FunctionCall(typing.NamedTuple):
    name: str
    args: List[str]

    @classmethod
    def generate(self, kind):
        if not functions[kind]: # fallback
            return generate_literal(kind)

        f = random.choice(functions[kind])
        return FunctionCall(f.name, [
            generate(arg_kind)
            for arg_kind in f.arg_types
        ])

    def render_kju(self):
        return '%s(%s)' % (self.name, ', '.join(arg.render_kju() for arg in self.args))

    def render_python(self):
        return '%s(%s)' % (self.name, ', '.join(arg.render_python() for arg in self.args))

class ReadVariable(typing.NamedTuple):
    name: str

    @classmethod
    def generate(self, kind):
        if not variables[kind]: # fallback
            return generate_literal(kind)

        f = random.choice(variables[kind])
        return ReadVariable(f)

    def render_kju(self):
        return self.name

    def render_python(self):
        return self.name

class Binop(typing.NamedTuple):
    op: str
    left: str
    right: str

    def render_kju(self):
        return '(%s %s %s)' % (self.left.render_kju(), self.op, self.right.render_kju())

    def render_python(self):
        op = {
            '||': 'or',
            '&&': 'and',
        }.get(self.op, self.op)

        r = '(%s %s %s)' % (self.left.render_python(), op, self.right.render_python())
        if op in int_bin_ops:
            return 'handle_overflow(%s)' % r
        else:
            return r

    @classmethod
    def generate(self, kind):
        if kind == 'bool':
            if random.randrange(2) == 0:
                return Binop(
                    random.choice(bool_bin_ops),
                    generate('bool'),
                    generate('bool'),
                )
            else:
                return Binop(
                    random.choice(compare_bin_ops),
                    generate('int'),
                    generate('int'),
                )
        elif kind == 'int':
            return Binop(
                random.choice(int_bin_ops),
                generate('int'),
                generate('int'),
            )

class IntValue(typing.NamedTuple):
    value: int

    def render_kju(self): return '(%s)' % (self.value)
    def render_python(self): return '(%s)' % (self.value)

    @classmethod
    def generate(self):
        return IntValue(random.randrange(-10, 10))

class BoolValue(typing.NamedTuple):
    value: bool

    def render_kju(self): return ['false', 'true'][self.value]
    def render_python(self): return str(self.value)

    @classmethod
    def generate(self):
        return BoolValue(random.choice([True, False]))

def generate_literal(kind):
    return {'int': IntValue, 'bool': BoolValue}[kind].generate() # type: ignore

def generate(kind):
    global expr_depth
    if expr_depth > 5:
        return generate_literal(kind)

    try:
        expr_depth += 1

        r = random.randrange(7)

        if expr_depth < 2 and kind == 'bool':
            return Binop.generate(kind)

        if r in (0, 1, 2):
            return Binop.generate(kind)

        if r == 3:
            return FunctionCall.generate(kind)

        if r in (4, 5):
            return ReadVariable.generate(kind)

        return generate_literal(kind)
    finally:
        expr_depth -= 1

class DeclareVariable(typing.NamedTuple):
    name: str
    expr: Any
    kind: str

    @classmethod
    def generate(self):
        kind = random.choice(['bool', 'int', 'int'])
        name = 'v%s%d' % (kind, len(variables[kind]))
        expr = generate(kind)
        variables[kind].append(name)
        return DeclareVariable(name, expr, kind)

    def render_kju(self):
        return 'var %s: %s = %s;' % (self.name, self.kind.capitalize(), self.expr.render_kju())

    def render_python(self):
        return '%s = %s' % (self.name, self.expr.render_python())

class Print(typing.NamedTuple):
    expr: Any

    @classmethod
    def generate(self):
        return Print(generate('int'))

    def render_kju(self):
        return 'write(%s);' % (self.expr.render_kju())

    def render_python(self):
        return 'print(%s)' % (self.expr.render_python())

def generate_stmt():
    if block_depth > 3:
        return DeclareVariable.generate()

    r = random.randrange(5)
    if r == 0:
        return If.generate()
    if r == 1:
        return Print.generate()

    return DeclareVariable.generate()

def indent(x):
    return '    ' + x.replace('\n', '\n    ')

class Block(typing.NamedTuple):
    stmts: List[Any]

    @classmethod
    def generate(self):
        global variables, block_depth
        variables_copy = copy.deepcopy(variables)
        block_depth += 1
        stmts = []

        for i in range(random.randrange(5)):
            stmts.append(generate_stmt())

        variables = variables_copy
        block_depth -= 1
        return Block(stmts)

    def render_kju(self):
        return '\n'.join(s.render_kju() for s in self.stmts)

    def render_python(self):
        if not self.stmts: return 'pass'
        return '\n'.join(s.render_python() for s in self.stmts)

class If(typing.NamedTuple):
    cond: Any
    then: Any
    else_: Any

    @classmethod
    def generate(self):
        cond = generate('bool')
        then = Block.generate()
        else_ = Block.generate()
        return If(cond, then, else_)

    def render_kju(self):
        return 'if (%s) then {\n%s\n} else {\n%s\n};' % (self.cond.render_kju(), indent(self.then.render_kju()), indent(self.else_.render_kju()))

    def render_python(self):
        return 'if %s:\n%s\nelse:\n%s\n' % (self.cond.render_python(), indent(self.then.render_python()), indent(self.else_.render_python()))

out = sys.argv[1]

for i in range(10):
    Function.generate()

variables = {'bool': [], 'int': []}
main = Block([
    generate_stmt() for i in range(30)
])

with open('%s.kju' % out, 'w') as w:
    w.write('fun write(a: Int): Unit import\n')
    for fun in all_functions:
        w.write(fun.render_kju() + '\n\n')

    w.write('fun kju(): Unit {\n    %s\n}' % indent(main.render_kju()))

with open('%s.py' % out, 'w') as w:
    w.write('''
class InvalidCode(Exception): pass

def handle_overflow(val):
    if val >= 2**63 or val < -2**63:
        raise InvalidCode()

    return val
''')

    for fun in all_functions:
        w.write(fun.render_python() + '\n\n')

    w.write(main.render_python())
