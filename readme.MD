#Dir2
**v1.01**

##Syntax: dir2 DIR/WILD [OPT ..]
*Example:*
dir2 obj\*.dll -s
*Example:*
dir2 bin *.exe *.dll -s --size-beyond=10m

##Options:
| Shortcut | Option           | Example or Info         | Shortcut Example |
| -------- | ------           | ---------------         | ---------------- |
|          | --cfg-off        | see --help=cfg          |                  |
|          | --utf8 	      | see --help=utf8         |                  |
|          | --regex          |                         |                  |
|          | --case-sensitive |                         |                  |
|          | --create-date    |                         |                  |
|          | --relative       |                         |                  |
|          | --count-comma    |                         |                  |
|          | --size-beyond    | --size-beyond=9k        |                  |
|          | --size-within    | --size-within=10m       |                  |
|          | --date-beyond    | --date-beyond=3h        |                  |
|          | --date-within    | --date-within=9d        |                  |
|          | --no-ext         | --no-ext=only           |                  |
|          | --hidden         | --hidden=only           |                  |
|          | --size-format    | --size-format=short     |                  |
|          | --count-width    | --count-width=10        |                  |
|          | --date-format    | --date-format=MM.dd     |                  |
|          | --total          | --total=only            |                  |
|          | --hide           | --hide=date             |                  |
| -o,      | --sort           | --sort=size             | -o date          |
|          | --dir            | --dir=sub               |                  |
|          | --sum            | --sum=ext               |                  |
| -x,      | --excl-file      | --excl-file=*.dll,*.pdb | -x *.exe,*.dat   |
| -X,      | --excl-dir       | --excl-dir=bin,obj      | -X bin,obj       |

##Other Shortcut:
| Shortcut | Stand for              | Description                    |
| -------- | ---------              | -----------                    |
| -s       | --dir=sub              | Recursively sub-directory      |
| -f       | --dir=off              | List file only                 |
| -d       | --dir=only             | List dir only                  |
| -T       | --dir=tree             | List dir tree                  |
| -c       | --case-sensitive       |                                |
| -b       | --total=off	    | List filename (with path) only |
|          | --hide=size,date,count |                                |
| -t       | --total=only           | Display total line only        |
  
Yung, Chun Kau <yung.chun.kau@gmail.com>
2021-10-16
