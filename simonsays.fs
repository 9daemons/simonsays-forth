\leds
4 constant red
8 constant yellow
16 constant green
32 constant blue

: random-color
    tick
    dup 0 = if drop red else
    dup 1 = if drop yellow else
    dup 2 = if drop green else
    drop blue then then then ;

\sonido
200 constant grave
150  constant medio
100 constant agudo
50  constant muyagudo

: pausa ( n -- ) 0 do loop ;
: sonido ( ciclos p -- ) 0 do on dup pausa off dup pausa loop drop ;

: on  ( -- ) portb c@ 16 or portb c! ;
: off ( -- ) portb c@ 239 and portb c! ;

$24 constant ddrb
$25 constant portb

: tono ( duracion tono -- )
    0 do
        on dup pausa
        off dup pausa
    loop 
    drop 
    off ;

: nota1 grave 50 tono ;
: nota2 medio 100 tono ;
: nota3 agudo 150 tono ;
: nota4 muyagudo 200 tono ;

: portD portd io! ;
: tick $46 c@ 4 mod ;

: sonido-random
    tick
    dup 0 = if drop nota1 else
    dup 1 = if drop nota2 else
    dup 2 = if drop nota3 else
    drop nota4 then then then ;

\ 5. Selección aleatoria de color


\ 6. El juego
: playgame
    begin
        0 portD
        1000 ms
        sonido-random
        random-color portD
        1000 ms
    key? until ;
