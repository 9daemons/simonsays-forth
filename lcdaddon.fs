$B8 constant TWBR              \ I2C clock speed register
$B9 constant TWSR              \ I2C prescaler + status register
$BB constant TWDR              \ I2C data register (byte to send/receive)
$BC constant TWCR              \ I2C control: bit7=TWINT bit5=START bit4=STOP bit2=ENABLE
$4E constant LCD-ADDR          \ PCF8574 address (0x27 << 1), change to $7E if 0x3F
: i2c-init  ( -- )       72 TWBR io! 0 TWSR io! ;           \ ~100kHz clock
: i2c-wait  ( -- )       begin TWCR io@ 128 and until ;     \ Spin until hardware done
: i2c-start ( -- )       164 TWCR io! i2c-wait ;            \ 128+32+4 = clear flag + START + enable
: i2c-stop  ( -- )       148 TWCR io! ;                     \ 128+16+4 = clear flag + STOP + enable
: i2c-write ( byte -- )  TWDR io! 132 TWCR io! i2c-wait ;   \ 128+4 = clear flag + enable, then wait
: lcd-nibble ( data rs -- )    \ Send 4 bits to LCD via PCF8574
    or 8 or                    \ Merge data + RS + backlight (bit 3)
    i2c-start LCD-ADDR i2c-write
    dup 251 and i2c-write 1 ms \ Enable LOW  (251 clears bit 2)
    dup 4 or i2c-write 2 ms   \ Enable HIGH (4 sets bit 2, LCD reads here)
    251 and i2c-write 1 ms     \ Enable LOW  (falling edge clocks it in)
    i2c-stop ;
: lcd-byte ( byte rs -- )     \ Send 8 bits as two nibbles, high first
    >r dup 240 and r@ lcd-nibble
    4 lshift 240 and r> lcd-nibble ;
: lcd-cmd  ( cmd -- )  0 lcd-byte 5 ms ;   \ Send command (RS=0)
: lcd-char ( char -- ) 1 lcd-byte 1 ms ;   \ Send character (RS=1)
: lcd-init ( -- )              \ Datasheet-mandated startup sequence
    i2c-init 50 ms             \ Init I2C, wait for LCD power-on
    $30 0 lcd-nibble 10 ms     \ Wake 1 — force known state
    $30 0 lcd-nibble 5 ms      \ Wake 2
    $30 0 lcd-nibble 1 ms      \ Wake 3
    $20 0 lcd-nibble 1 ms      \ Switch to 4-bit mode
    $28 lcd-cmd                \ 2 lines, 5x8 font
    $01 lcd-cmd 20 ms          \ Clear display (slow command)
    $06 lcd-cmd                \ Entry mode: cursor moves right
    $0C lcd-cmd ;              \ Display ON, cursor OFF
: lcd-clear ( -- ) $01 lcd-cmd 5 ms ;                \ Clear screen
: lcd-set-cursor ( col row -- ) 64 * + $80 + lcd-cmd ; \ Position cursor
: lcd-string ( addr len -- )   \ Print a string to LCD
    0 do dup i + c@ lcd-char loop drop ;
: lcd-digit ( n -- ) 48 + lcd-char ;  \ Convert 0-9 to ASCII and print
: lcd-number ( n -- )          \ Print number 0-999, no leading zeros
    dup 100 >= if dup 100 / lcd-digit then
    dup 10 >= if dup 10 / 10 mod lcd-digit then
    10 mod lcd-digit ;
: print-lcd ( addr len col row -- ) lcd-set-cursor lcd-string ; \ Print text at position
: print-lcd-number ( n col row -- ) lcd-set-cursor lcd-number ; \ Print number at position
: lcd-game-over ( -- )
    lcd-clear
    s" Game Over" 0 0 print-lcd ;
: lcd-score ( -- )
    s" Score: " 0 0 print-lcd ;
