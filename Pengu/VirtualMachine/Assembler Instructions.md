| instruction         | bytes                 | description
|---------------------|-----------------------|--------------
| ORG                 | -                     | sets the starting point of the program (last 1-2 bytes)
| @addr               | -                     | continues the assembly at the given address
| .label              | -                     | marks the current assembly address with a label
| DB I8               | i8                    | writes the byte at the current assembly address
| NOP                 | 00                    |
| MOV REG, I8         | 01 reg i8             |
| MOV REG, REG        | 02 reg:4\|reg:4       |
| MOV REG, [I8]       | 03 reg i8             |
| MOV REG, [REG]      | 04 reg:4\|reg:4       |
| MOV [I8], I8        | 05 i8 i8              |
| MOV [I8], REG       | 06 i8 reg             |
| MOV [I8], [I8]      | 07 i8 i8              |
| MOV [I8], [REG]     | 08 i8 reg             |
| MOV [REG], I8       | 09 reg i8             |
| MOV [REG], REG      | 0a reg:4\|reg:4       |
| MOV [REG], [I8]     | 0b reg i8             |
| MOV [REG], [REG]    | 0c reg:4\|reg:4       |
| ADDI REG, I8        | 0d reg i8             |
| ADDI REG, REG       | 0e reg:4\|reg:4       |
| SUBI REG, I8        | 0f reg i8             |
| SUBI REG, REG       | 10 reg:4\|reg:4       |
| MULI REG, I8        | 11 reg i8             |
| MULI REG, REG       | 12 reg:4\|reg:4       |
| INT I8              | 13 i8                 |
| INT reg             | 14 i8                 |
| DIVI REG, I8        | 15 reg i8             |
| DIVI REG, REG       | 16 reg:4\|reg:4       |
| MODI REG, I8        | 17 reg i8             |
| MODI REG, REG       | 18 reg:4\|reg:4       |
| JMP I8              | 19 i8                 |
| JMP $ + I8          | 1a i8                 |
| JMP REG             | 1b i8                 |
| JMP $ + REG         | 1c i8                 |
| CMP I8, I8          | 1d i8 i8              | sets the compare flag to -1 / 0 / 1
| CMP REG, I8         | 1e reg i8             | sets the compare flag to -1 / 0 / 1
| CMP REG, REG        | 1f reg:4\|reg:4       | sets the compare flag to -1 / 0 / 1
| JL I8               | 20 i8                 |
| JL $ + I8           | 21 i8                 |
| JL REG              | 22 reg                |
| JL $ + REG          | 23 reg                |
| JLE I8              | 24 i8                 |
| JLE $ + I8          | 25 i8                 |
| JLE REG             | 26 reg                |
| JLE $ + REG         | 27 reg                |
| JG I8               | 28 i8                 |
| JG $ + I8           | 29 i8                 |
| JG REG              | 2a reg                |
| JG $ + REG          | 2b reg                |
| JGE I8              | 2c i8                 |
| JGE $ + I8          | 2d i8                 |
| JGE REG             | 2e reg                |
| JGE $ + REG         | 2f reg                |
| JE I8               | 30 i8                 |
| JE $ + I8           | 31 i8                 |
| JE REG              | 32 reg                |
| JE $ + REG          | 33 reg                |
| JNE I8              | 34 i8                 |
| JNE $ + I8          | 35 i8                 |
| JNE REG             | 36 reg                |
| JNE $ + REG         | 37 reg                |
