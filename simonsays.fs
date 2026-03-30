\variables
variable tick
variable pushed-button
variable last-led
variable contador

\constants
\leds
1 constant redb
2 constant yellowb
4 constant greenb
8 constant blueb
\masks
15 constant button-mask \ Bits 0,1,2,3 (1+2+4+8 = 15)
240 constant button-clear \ Inversa para entradas (255 - 15 = 240)
60 constant leds-mask    \ Bits 2,3,4,5 (4+8+16+32 = 60)
195 constant leds-clear
\leds
4 constant red
8 constant yellow
16 constant green
32 constant blue
\sound
200 constant low
150  constant medium
100 constant high
50  constant veryhigh

\basic-functions
: tickstore $46 c@ 4 mod tick ! ;
: stop ( n -- ) 0 do loop ;
: on  ( -- ) portb c@ 16 or portb c! ;
: off ( -- ) portb c@ 239 and portb c! ;
: tone ( time , tone -- ) 0 do on dup stop off dup stop loop drop off ;
: note1 low 50 tone ;
: note2 medium 100 tone ;
: note3 high 150 tone ;
: note4 veryhigh 200 tone ;
: leds-off 0 portd io! ;
: buttons-input ddrB c@ button-clear and ddrB c! ;
: leds-output ddrD c@ leds-mask or ddrD c! ;
\important-functions
: random-color
    tick @
    dup 0 = if drop red else
    dup 1 = if drop yellow else
    dup 2 = if drop green else
    drop blue then then then ;
: random-sound
    tick @
    dup 0 = if drop note1 else
    dup 1 = if drop note2 else
    dup 2 = if drop note3 else
    drop note4 then then then ;
: configuration-restart
    leds-output
    buttons-input
    leds-off ;
: leer-botones ( -- n )
    pinB c@ invert button-mask and ;
: actualizar-leds ( n -- )
    4 * \ Multiplica por 4 para alinear con los LEDs
    leds-off
    or                       \ Mezcla el nuevo estado
    portd c! ;               \ Lo envía al hardware
: capturar-boton ( -- )
    begin 
        leer-botones dup 0> 
    until
    dup 1 = if drop 0 else
    dup 2 = if drop 1 else
    dup 4 = if drop 2 else
    drop 3 then then then
    pushed-button !
    begin 
        leer-botones 0= 
    until
    50 ms ;
: comprobar-jugada ( -- flag )
    pushed-button @ tick @ = if 
        \ --- ACERTÓ ---
        contador @ 1 + contador !  \ Le suma 1 a la variable contador
        0                          \ Dejamos un 0 en la pila: UNTIL repetirá el bucle
    else
        \ --- FALLÓ ---
        1                          \ Dejamos un 1 en la pila: UNTIL romperá el bucle
    then 
;

: playgame
    configuration-restart
    begin
        tickstore
        1000 ms
        random-color portD
        50 ms
        random-sound
        1000 ms
        leds-off
        capturar-boton
    key? until 
    0 portD ;








