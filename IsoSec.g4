grammar IsoSec;

WS : [ \t\f\r\n] -> skip;

EndLine : ';';

LPar : '(';
RPar : ')';
LSBra : '[';
RSBra : ']';

RetOp : 'return';
NewOp : 'create';

IncOp : '++';
DecOp : '--';

PowerOp : 'pow';
PlusOp : '+';
MinusOp : '-';
ModOp : 'mod';
TimesOp : '*';
DivOp : '/';

LBSOp : '<<';
RBSOp : '>>';

LEOp : '<=';
GEOp : '>=';
LessOp : '<';
GreatOp : '>';
EqualOp : '==';
NEqualOp : '!=';
AssignOp : '=';

LAndOp : '&&';
LOrOp : '||';
LNotOp : '!';

BAndOp : '&';
BXOrOp : '^';
BOrOp : '|';
BNotOp : '~';

DotOp : '.';
Comma : ',';

Float : ([0-9]+'.'[0-9]+'f' | [0-9]+'f');
Double : [0-9]+'.'[0-9]+;
Int : [0-9]+;
Bool : 'true' | 'false';
Char : '\'' '\\'?. '\'';
String : '$'?'"' .*? '"';

Name : [_a-zA-Z][_a-zA-Z0-9]*;

All : [^;=]+;

BlockComment : '/*' .*? '*/' -> skip;
LineComment : '//' ~[\r\n]* -> skip;

program : (funcDeclaration | decLine)*;

typeName : compName ('<' typeName (Comma typeName)* '>')? (LSBra RSBra)? '?'?;
funcDeclaration : type=typeName 'function' name=Name LPar declararionArgs? RPar line* 'end';

declararionArgs : declaration (Comma declaration)?;

line : lstat=stat EndLine?;
decLine : dec=declaration EndLine?;

stat : RetOp exp #returnStat
     | declaration #decStat
     | ctrlStruct #ctrlStat
     | exp #expStat;

compName : compName DotOp Name
         | Name;

declarationBase : type=typeName name=Name;
declaration : baseDec=declarationBase (AssignOp value=exp)?;

ctrlStruct : 'if' cond=exp 'then' line* ifElse* else? 'end' #if
           | 'while' cond=exp 'do' line* 'end' #while
           | 'do' line* 'while' cond=exp #doWhile
           | 'for' (dvar=declaration | ivar=exp) ';' cond=exp ';' iexp=exp 'do' line* 'end' #for;

ifElse : 'else' 'if' cond=exp 'then' line*;
else : 'else' line*;

exp : LPar type=typeName RPar right=exp
    | left=exp op=( IncOp | DecOp )
    | op=( IncOp | DecOp | LNotOp | BNotOp | MinusOp ) right=exp
    | left=exp op=PowerOp right=exp
    | left=exp op=( TimesOp | DivOp | ModOp ) right=exp
    | left=exp op=( PlusOp | MinusOp ) right=exp
    | left=exp op=( LBSOp | RBSOp ) right=exp
    | left=exp op=( GreatOp | LessOp | GEOp | LEOp ) right=exp
    | left=exp op=( EqualOp | NEqualOp ) right=exp
    | left=exp op=BAndOp right=exp
    | left=exp op=BXOrOp right=exp
    | left=exp op=BOrOp right=exp
    | left=exp op=LAndOp right=exp
    | left=exp op=LOrOp right=exp
    | left=exp op=AssignOp right=exp
    | LPar left=exp RPar
    | compVar;
    
compVar : left=compVar DotOp right=atomVar
        | at=atom;

atom : newObj
     | atomVar
     | const;

atomVar : func
        | var;

var : name=Name LSBra args RSBra
    | name=Name;

func : name=Name LPar RPar
     | name=Name LPar args RPar;

args : exp (Comma exp)*;

const : Bool
      | Char
      | String
      | Float
      | Double
      | Int;

newObj : NewOp typeName LPar args? RPar; 