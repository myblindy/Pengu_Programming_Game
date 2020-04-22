| instruction     | base cycles | bytes                 |
|-----------------|-------------|-----------------------|
| MOV REG, I8     |      1      | 00 reg i8             |
| MOV REG, REG    |      1      | 01 reg:4\|reg:4       |
| MOV REG, [I8]   |      1      | 02 reg i8             |
| MOV REG, [REG]  |      1      | 03 reg:4\|reg:4       |
| MOV [I8], I8    |      1      | 04 i8 i8              |
| MOV [I8], REG   |      1      | 05 i8 reg             |
| MOV [I8], [I8]  |      1      | 06 i8 i8              |
| MOV [I8], [REG] |      1      | 07 i8 reg             |
| MOV [REG], I8   |      1      | 08 reg i8             |
| MOV [REG], REG  |      1      | 09 reg:4\|reg:4       |
| MOV [REG], [I8] |      1      | 0a reg i8             |
| MOV [REG], [REG]|      1      | 0b reg:4\|reg:4       |
| ADDI REG, I8    |      1      | 0c reg i8             |
| ADDI REG, REG   |      1      | 0d reg:4\|reg:4       |
| ADDI REG, [I8]  |      1      | 0e reg i8             |
| ADDI REG, [REG] |      1      | 0f reg:4\|reg:4       |
| SUBI REG, I8    |      1      | 10 reg i8             |
| SUBI REG, REG   |      1      | 11 reg:4\|reg:4       |
| SUBI REG, [I8]  |      3      | 12 reg i8             |
| SUBI REG, [REG] |      1      | 13 reg:4\|reg:4       |
| MULI REG, I8    |      3      | 14 reg i8             |
| MULI REG, REG   |      3      | 15 reg:4\|reg:4       |
| MULI REG, [I8]  |      3      | 16 reg i8             |
| MULI REG, [REG] |      3      | 17 reg:4\|reg:4       |
