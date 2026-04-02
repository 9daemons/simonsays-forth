\ === LCD I2C ===
$BC constant TWCR
: i2c-wait
  begin TWCR io@ 128 and until ;
: i2c-write
  $BB io! 132 TWCR io! i2c-wait ;
: lcd-nib
  or 8 or
  164 TWCR io! i2c-wait $4E i2c-write
  dup 251 and i2c-write 1 ms
  dup 4 or i2c-write 2 ms
  251 and i2c-write 1 ms
  148 TWCR io! ;
: lcd-byte
  >r dup 240 and r@ lcd-nib
  4 lshift 240 and r> lcd-nib ;
: lcd-cmd 0 lcd-byte 5 ms ;
: lcd-char 1 lcd-byte 1 ms ;
: lcd-init
  72 $B8 io! 0 $B9 io! 50 ms
  $30 0 lcd-nib 10 ms
  $30 0 lcd-nib 5 ms
  $30 0 lcd-nib 1 ms
  $20 0 lcd-nib 1 ms
  $28 lcd-cmd
  $01 lcd-cmd 20 ms
  $06 lcd-cmd
  $0C lcd-cmd ;
: lcd-clear $01 lcd-cmd 5 ms ;
: lcd-pos
  64 * + $80 + lcd-cmd ;
: lcd-str
  0 do dup i + c@ lcd-char loop drop ;
: lcd-num
  dup 100 >= if dup 100 / 48 + lcd-char then
  dup 10 >= if dup 10 / 10 mod 48 + lcd-char then
  10 mod 48 + lcd-char ;

\ === SIMON SAYS ===
variable tick
variable pbtn
variable score
variable fail
create seq 30 allot
create colors 4 c, 8 c, 16 c, 32 c,
create tones 200 c, 150 c, 100 c, 50 c,
create bmap
  0 c, 0 c, 1 c, 0 c, 2 c, 0 c, 0 c, 0 c, 3 c,
: stop 0 do loop ;https://github.com/cruftex/ec4th aqui tienes 
: on portb io@ 16 or portb c! ;
: off portb io@ 239 and portb c! ;
: tone
  millis
  begin
    on over stop
    off over stop
    millis over - 1000 >
  until
  2drop off ;
: errtone 150 tone 100 ms 200 tone ;
: leds-off
  portd io@ 195 and portd c! ;
: show-led
  leds-off portd io@ or portd c! ;
: btn-in
  ddrB io@ 240 and ddrB c! ;
: led-out
  ddrD io@ 60 or ddrD c! ;
: rnd-clr tick @ colors + c@ ;
: rnd-snd tick @ tones + c@ tone ;
: cfg led-out btn-in leds-off ;
: rd-btn
  pinB io@ invert 15 and ;
: get-btn
  0 begin drop rd-btn dup until
  bmap + c@ pbtn !
  begin rd-btn 0= until
  20 ms ;
: new-lvl
  $46 io@ 4 mod
  seq score @ + c! ;
: play-seq
  score @ 1 + 0 do
    seq i + c@ tick !
    rnd-clr show-led rnd-snd
    300 ms leds-off 200 ms
  loop ;
: chk-seq
  0 fail !
  score @ 1 + 0 do
    get-btn pbtn @
    seq i + c@ = if
    else
      1 fail ! leave
    then
  loop
  fail @ ;
: fail-ann
  255 portd io!
  errtone 300 ms leds-off ;
: playgame
  lcd-init cfg 0 score !
  begin
    lcd-clear
    s" Round: " lcd-str score @ lcd-num
    1000 ms
    new-lvl play-seq chk-seq
    dup 0= if
      score @ 1 + score !
    then
    key? or
  until
  fail-ann lcd-clear s" Game Over! " lcd-str ;