# KJU, the supreme language

aka kompilatory-2019-1

## Quick start

### Manual

Using `dotnet` command.

Eg. on Linux.

#### Run application

```
$ cd src/KJU.Application
$ dotnet run
```

or (Unix only)

```
./kju
```

#### Build core

```
$ cd src/KJU.Core
$ dotnet build
```

#### Test

```
$ cd src/KJU.Tests
$ dotnet test /p:CollectCoverage=true
```

### Visual Studio

- Install .Net Core 2.1 on Visual Studio

- Open `KJU.sln`

- Use Visual Studio features

## Documentation

For more documentation visit doc/ folder.

## AST dot graph generation

### Prerequisites

- graphviz installed

### Example procedure

```
$ dotnet run --project src/KJU.Application --gen-ast-dot sources.kju
$ dot -T pdf -o sources.ast.pdf sources.ast.dot
```
