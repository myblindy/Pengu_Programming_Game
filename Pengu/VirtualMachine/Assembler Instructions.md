| instruction         | base cycles | bytes                 |
|---------------------|-------------|-----------------------|
| ORG                 |      -      | -                     |
| @addr               |      -      | -                     |
| DB I8               |      -      | i8                     |
| END                 |      1      | 00                    |
| MOV REG, I8         |      1      | 01 reg i8             |
| MOV REG, REG        |      1      | 02 reg:4\|reg:4       |
| MOV REG, [I8]       |      1      | 03 reg i8             |
| MOV REG, [REG]      |      1      | 04 reg:4\|reg:4       |
| MOV [I8], I8        |      1      | 05 i8 i8              |
| MOV [I8], REG       |      1      | 06 i8 reg             |
| MOV [I8], [I8]      |      1      | 07 i8 i8              |
| MOV [I8], [REG]     |      1      | 08 i8 reg             |
| MOV [REG], I8       |      1      | 09 reg i8             |
| MOV [REG], REG      |      1      | 0a reg:4\|reg:4       |
| MOV [REG], [I8]     |      1      | 0b reg i8             |
| MOV [REG], [REG]    |      1      | 0c reg:4\|reg:4       |
| ADDI REG, I8        |      1      | 0d reg i8             |
| ADDI REG, REG       |      1      | 0e reg:4\|reg:4       |
| ADDI REG, [I8]      |      1      | 0f reg i8             |
| ADDI REG, [REG]     |      1      | 10 reg:4\|reg:4       |
| SUBI REG, I8        |      1      | 11 reg i8             |
| SUBI REG, REG       |      1      | 12 reg:4\|reg:4       |
| SUBI REG, [I8]      |      3      | 13 reg i8             |
| SUBI REG, [REG]     |      1      | 14 reg:4\|reg:4       |
| MULI REG, I8        |      3      | 15 reg i8             |
| MULI REG, REG       |      3      | 16 reg:4\|reg:4       |
| MULI REG, [I8]      |      3      | 17 reg i8             |
| MULI REG, [REG]     |      3      | 18 reg:4\|reg:4       |
| INT I8              |      ?      | 19 i8                 |
| DIVI REG, I8        |      3      | 1a reg i8             |
| DIVI REG, REG       |      3      | 1b reg:4\|reg:4       |
| DIVI REG, [I8]      |      3      | 1c reg i8             |
| DIVI REG, [REG]     |      3      | 1d reg:4\|reg:4       |
| MODI REG, I8        |      3      | 1e reg i8             |
| MODI REG, REG       |      3      | 1f reg:4\|reg:4       |
| MODI REG, [I8]      |      3      | 20 reg i8             |
| MODI REG, [REG]     |      3      | 21 reg:4\|reg:4       |
