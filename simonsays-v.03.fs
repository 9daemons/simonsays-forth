\ variables
variable tick
variable pbtn
variable last-led
variable score
variable fail
create seq 30 allot
create colors 4 c, 8 c, 16 c, 32 c,
create tones 200 c, 150 c, 100 c, 50 c,
create bmap 0 c, 0 c, 1 c, 0 c, 2 c, 0 c, 0 c, 0 c, 3 c,
$27 constant lcd !

\ basic-functions
: stop ( n -- ) 0 do loop ;
: on  ( -- ) portb io@ 16 or portb c! ;
: off ( -- ) portb io@ 239 and portb c! ;
: tone
    millis
    begin
        on over stop
        off over stop
        millis over - 1000 >
    until
    2drop off ;
: errtone 150 tone 100 ms 200 tone ;
: leds-off portd io@ 195 and portd c! ;
: show-led leds-off portd io@ or portd c! ;
: btn-in ddrB io@ 240 and ddrB c! ;
: led-out ddrD io@ 60 or ddrD c! ;
: rnd-clr tick @ colors + c@ ;
: rnd-snd tick @ tones + c@ tone ;
: cfg led-out btn-in leds-off ;
: rd-btn pinB io@ invert 15 and ;
: get-btn
    0 begin drop rd-btn dup until
    bmap + c@ pbtn !
    begin rd-btn 0= until
    20 ms ;
: new-lvl
    $46 io@ 4 mod
    seq score @ + c! ;
: play-seq
    score @ 1 + 0 
    do
        seq i + c@ tick !
        rnd-clr show-led
        rnd-snd 
        300 ms leds-off 200 ms
    loop ;
: chk-seq
    0 fail !
    score @ 1 + 0 
    do
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
    lcd lcd-init cfg 0 score !
    begin
        lcd lcd-clear
        s" Round: " lcd lcd-type 
        score @ lcd lcd-number
        1000 ms
        new-lvl play-seq chk-seq
        dup 0= if
            1 score +!
            drop 0
        else
            drop 1
        then
        key? or
    until
    fail-ann 
    lcd lcd-clear 
    s" Game Over! " lcd lcd-type ;