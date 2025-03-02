function list_child_processes () {
    local ppid=$1;
    local current_children=$(pgrep -P $ppid);
    local local_child;
    if [ $? -eq 0 ];
    then
        for current_child in $current_children
        do
          local_child=$current_child;
          list_child_processes $local_child;
          echo $local_child;
        done;
    else
      return 0;
    fi;
}

ps 10590;
while [ $? -eq 0 ];
do
  sleep 1;
  ps 10590 > /dev/null;
done;

for child in $(list_child_processes 10601);
do
  echo killing $child;
  kill -s KILL $child;
done;
rm /Users/ritabriosa/Desktop/PV/VitalEase-1/PV_VitalEase/VitalEase.Server/bin/Debug/net8.0/596f927bec1349fa8260c39b994ab6be.sh;
