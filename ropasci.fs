variable user-choice
variable random-choice
create bmap   0 c, 0 c,  1 c,  0 c, 2 c, 0 c, 0 c, 0 c, 0 c,
$27 constant lcd !

: cfg-btns
    ddrB io@ 248 and ddrB c! 
    portB io@ 7 or portB c! ; 

: rd-btns
    pinB io@ invert 7 and ; 

: get-user
    0 begin drop rd-btns dup until
    bmap + c@ user-choice !
    begin rd-btns 0= until
    20 ms ;

: choice
    dup 0 = if s" Rock"  lcd lcd-type then
    dup 1 = if s" Paper"   lcd lcd-type then
    dup 2 = if s" Scissors"  lcd lcd-type then
    drop ;

: show-result
    lcd lcd-clear
    over over = if
        s" Tie!" lcd lcd-type
    else
        over 0 = over 2 = and if s" You won! :)" else
        over 1 = over 0 = and if s" You won! :)" else
        over 2 = over 1 = and if s" You won! :)" else
        s" You lost :(" 
        then then then
        lcd lcd-type
    then
    2drop ;

: play
    lcd lcd-init cfg-btns
    begin
        lcd lcd-clear
        s" 1.Rock 2.Paper " lcd lcd-type
        0 1 lcd lcd-at
        s" 3.Scissors " lcd lcd-type
        get-user 
        lcd lcd-clear
        s" CPU thinking..." lcd lcd-type
        750 ms
        $46 io@ 3 mod random-choice !
        lcd lcd-clear
        s" You:" lcd lcd-type user-choice @ choice
        0 1 lcd lcd-at
        s" CPU:" lcd lcd-type random-choice @ choice
        2000 ms
        user-choice @ random-choice @ show-result
        3000 ms
        key? 
    until
    lcd lcd-clear
    s" Bye!" lcd lcd-type ;