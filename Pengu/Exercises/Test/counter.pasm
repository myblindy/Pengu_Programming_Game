; temporary storage for 
; the current counter
.tmp db 0

org
mov r0 0

.loop
addi r0 1
modi r0 100
mov [.tmp] r0 

; first digit
divi r0 10
int 1
int 45

; second digit
mov r0 [.tmp]
modi r0 10
int 1
int 46

; restore and loop
mov r0 [.tmp]
jmp .loop