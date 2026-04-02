\ variables
variable tick
variable pushed-button
variable last-led
variable score
variable fail-flag
create sequence 100 allot
\ constants
\ leds
1 constant redb
2 constant yellowb
4 constant greenb
8 constant blueb
\ masks
15 constant button-mask \ (1+2+4+8 = 15)
240 constant button-clear \ (255 - 15 = 240)
60 constant leds-mask    \ (4+8+16+32 = 60)
195 constant leds-clear \ (255-60 = 195)
\ leds
4 constant red
8 constant yellow
16 constant green
32 constant blue
\ sound
200 constant low
150  constant medium
100 constant high
50  constant veryhigh

\ basic-functions
: randomNumber $46 io@ 4 mod tick ! ;
: stop ( n -- ) 0 do loop ;
: on  ( -- ) portb io@ 16 or portb c! ;
: off ( -- ) portb io@ 239 and portb c! ;
: tone ( tone -- )
    millis
    begin ( tone startingtime -- )
        on over stop 
        off over stop 
    millis ( tone startingtime currenttime -- )
    over - ( tone startingtime elapsedtime -- )
    1000 > ( tone startingtime f -- ) until ( tone startingtime -- ) 
    2drop
    off ;
: note1 low tone ;
: note2 medium tone ;
: note3 high tone ;
: note4 veryhigh tone ;
: errornote medium tone 100 ms low tone ;
: leds-off ( -- ) portd io@ leds-clear and portd c! ;
: show-led ( color -- ) leds-off portd io@ or portd c! ;
: buttons-input ddrB io@ button-clear and ddrB c! ;
: leds-output ddrD io@ leds-mask or ddrD c! ;

\ important-functions
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
: read-buttons ( -- n )
    pinB io@ invert button-mask and ;
: update-leds ( n -- )
    4 *
    leds-off
    or
    portd c! ;
: capture-button ( -- )
    begin 
        read-buttons 
        dup 0= if drop 0 else 1 then 
    until
    dup 1 = if drop 0 else
    dup 2 = if drop 1 else
    dup 4 = if drop 2 else
    drop 3 then then then
    pushed-button !
    begin 
        read-buttons 0= 
    until
    20 ms ;
: new-level ( -- )
    $46 io@ 4 mod
    sequence score @ + c! ;
: play-sequence ( -- )
    score @ 1 + 0 do
        sequence i + c@ tick !
        random-color show-led random-sound
        300 ms leds-off 200 ms
    loop ;
: check-sequence ( -- flag )
    0 fail-flag !
    score @ 1 + 0 do
        capture-button
        pushed-button @ 
        sequence i + c@ = if
        else
            1 fail-flag !
            leave
        then
    loop
    fail-flag @ ;   
: fail-announce
    255 portd io!
    errornote
    300 ms
    leds-off ;
: playgame
    configuration-restart
    0 score !
    lcd-init
    begin
        1000 ms
        lcd-score
        new-level
        play-sequence
        check-sequence 
        dup 0= if 
            score @ 1 + score !
        then
        key? or
    until 
    fail-announce
    lcd-game-over;