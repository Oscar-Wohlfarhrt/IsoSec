# IsoSec
 
IsoSec is a simple language transpiler from my custom source code to C#.

This project was created for the subject Compilers of the Universidad Gaston Dachary (UGD) on Argentina.

For executing IsoSec code you need to run: `IsoSec ./path/to/file`

For aditional info and syntax see [IsoSec Syntax](#isosec-syntax)

## IsoSec Command

Is recomended to add the executable file to the `PATH` enviromental variable of the operating system. 

This is the command help:
```
Description:
  Simple IsoSec language transpiller and compiler

Usage:
  IsoSec <--source> [<--args>...] [options]

Arguments:
  <--source>  The IsoSec source file to process
  <--args>    The arguments passed to the IsoSec program []

Options:
  -p, --parse <parse>  The file with a CSharp equivalent code
  -s, --show-cs        Shows the CSharp equivalent code [default: False]
  -u, --using <using>  The namespaces that CSharp compiler will use [default:
                       System|System.Collections|System.Collections.Generic]
  -hi, --hide-info     Hides command info lines [default: False]
  --version            Show version information
  -?, -h, --help       Show help and usage information
```

Aditionally this command suports arguments inside the file, these need to be in the first comment of the file, like this:
```
/*
    Author: Author Name
    Usings: Aditional C# usings to be included (they need to be installed on the system)
*/
```
or
```
//
// Author: Author Name
// Usings: Aditional C# usings to be included (they need to be installed on the system)
//
```
_Note: if you are using single line comments, they all need to be continous. Single and Multiline comments cannot be mixed._

## IsoSec Syntax

The language specifications are in this file:
- Spanish Version: [IsoSec: Un lenguaje de programación aislado y
seguro](./IsoSec%20Compiladores%20-%20Balbuena%2C%20Hillebrand%20y%20Wohlfarhrt.pdf)
