

instruction         | bytes                 | description
--------------------|-----------------------|--------------
ORG                 | -                     | sets the starting point of the program (last 1-2 bytes)
@addr               | -                     | continues the assembly at the given address
.label              | -                     | marks the current assembly address with a label
DB I8               | i8                    | writes the byte at the current assembly address
 NOP | 00  |
 MOV REG I8 | 01 reg:4\|reg:4  |
 MOV REG REG | 02 reg:4\|reg:4  |
 MOV REG PI8 | 03 reg:4\|reg:4  |
 MOV REG PREG | 04 reg:4\|reg:4  |
 MOV PI8 I8 | 05 i8 i8  |
 MOV PI8 REG | 06 i8 reg  |
 MOV PI8 PI8 | 07 i8 i8  |
 MOV PI8 PREG | 08 i8 reg  |
 MOV PREG I8 | 09 reg:4\|reg:4  |
 MOV PREG REG | 0a reg:4\|reg:4  |
 MOV PREG PI8 | 0b reg:4\|reg:4  |
 MOV PREG PREG | 0c reg:4\|reg:4  |
 INT I8 | 0d i8  |
 INT REG | 0e reg  |
 JMP I8 | 0f i8  |
 JMP $+I8 | 10 i8  |
 JMP REG | 11 reg  |
 JMP $+REG | 12 reg  |
 CMP I8 I8 | 13 i8 i8  |
 CMP REG I8 | 14 reg:4\|reg:4  |
 CMP REG REG | 15 reg:4\|reg:4  |
 JL I8 | 16 i8  |
 JL $+I8 | 17 i8  |
 JL REG | 18 reg  |
 JL $+REG | 19 reg  |
 JLE I8 | 1a i8  |
 JLE $+I8 | 1b i8  |
 JLE REG | 1c reg  |
 JLE $+REG | 1d reg  |
 JG I8 | 1e i8  |
 JG $+I8 | 1f i8  |
 JG REG | 20 reg  |
 JG $+REG | 21 reg  |
 JGE I8 | 22 i8  |
 JGE $+I8 | 23 i8  |
 JGE REG | 24 reg  |
 JGE $+REG | 25 reg  |
 JE I8 | 26 i8  |
 JE $+I8 | 27 i8  |
 JE REG | 28 reg  |
 JE $+REG | 29 reg  |
 JNE I8 | 2a i8  |
 JNE $+I8 | 2b i8  |
 JNE REG | 2c reg  |
 JNE $+REG | 2d reg  |
 PUSH I8 | 2e i8  |
 PUSH REG | 2f reg  |
 POP REG | 30 reg  |
 CALL I8 | 31 i8  |
 CALL $+I8 | 32 i8  |
 CALL REG | 33 reg  |
 CALL $+REG | 34 reg  |
 RET | 35  |
 SHL REG REG | 36 reg:4\|reg:4  |
 SHL REG I8 | 37 reg:4\|reg:4  |
 SHR REG REG | 38 reg:4\|reg:4  |
 SHR REG I8 | 39 reg:4\|reg:4  |
 OR REG REG | 3a reg:4\|reg:4  |
 OR REG I8 | 3b reg:4\|reg:4  |
 AND REG REG | 3c reg:4\|reg:4  |
 AND REG I8 | 3d reg:4\|reg:4  |
 XOR REG REG | 3e reg:4\|reg:4  |
 XOR REG I8 | 3f reg:4\|reg:4  |
 ADDI REG REG | 40 reg:4\|reg:4  |
 ADDI REG I8 | 41 reg:4\|reg:4  |
 SUBI REG REG | 42 reg:4\|reg:4  |
 SUBI REG I8 | 43 reg:4\|reg:4  |
 MULI REG REG | 44 reg:4\|reg:4  |
 MULI REG I8 | 45 reg:4\|reg:4  |
 DIVI REG REG | 46 reg:4\|reg:4  |
 DIVI REG I8 | 47 reg:4\|reg:4  |
 MODI REG REG | 48 reg:4\|reg:4  |
 MODI REG I8 | 49 reg:4\|reg:4  |
 NOT REG | 4a reg  |

