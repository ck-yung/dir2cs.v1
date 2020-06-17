v1.00 First Release

LICENSE information is added.

Syntax: dir2 DIR/WILD [OPT ..]

Syntax: dir2 [DIR] [WILD ..] [OPT ..]
OPT:
              --cfg-off 	see --help=cfg
                 --utf8 	see --help=utf8
       --case-sensitive
          --create-date
             --relative
          --count-comma
          --size-beyond=NUMBER
          --size-within=NUMBER
          --date-beyond=DATETIME
          --date-within=DATETIME
               --no-ext=excl|only
               --hidden=incl|only
          --size-format=long|short
          --count-width=NUMBER
          --date-format=FORMAT
                --total=off|only
                 --hide=size,date,count
  -o,            --sort=name|size|date|last|count
                  --dir=sub|off|only|tree
                  --sum=ext|dir
  -x,       --excl-file=WILD[,WILD,..]
  -X,        --excl-dir=WILD[,WILD,..]
  -n,            --name=WILD[,WILD,..]
Shortcut:
  -s => --dir=sub
  -f => --dir=off
  -d => --dir=only
  -T => --dir=tree
  -c => --case-sensitive
  -b => --total=off	--hide=size,date,count
  -t => --total=only
  

Yung, Chun Kau <yung.chun.kau@gmail.com>
2020-06-17
