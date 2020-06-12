// define a grammar called Hello
grammar BLanguageGrammar;

r   : 'hello' ID;
ID  : [a-z]+ ;
WS  : [ \t\r\n]+ -> skip ;