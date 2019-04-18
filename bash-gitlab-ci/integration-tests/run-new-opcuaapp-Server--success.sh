#!/bin/bash

set -euo pipefail

source bash-gitlab-ci/util-integration-tests.sh

VAR_COMMANDS[0]="oppo new opcuaapp -n my-app -t Server -u 127.0.0.1 -p 4840"
VAR_COMMANDS[1]="oppo new opcuaapp -n my-app -t Server -u 127.0.0.1 --port 4840"
VAR_COMMANDS[2]="oppo new opcuaapp -n my-app -t Server --url 127.0.0.1 -p 4840"
VAR_COMMANDS[3]="oppo new opcuaapp -n my-app -t Server --url 127.0.0.1 --port 4840"
VAR_COMMANDS[4]="oppo new opcuaapp -n my-app --type Server -u 127.0.0.1 -p 4840"
VAR_COMMANDS[5]="oppo new opcuaapp -n my-app --type Server -u 127.0.0.1 --port 4840"
VAR_COMMANDS[6]="oppo new opcuaapp -n my-app --type Server --url 127.0.0.1 -p 4840"
VAR_COMMANDS[7]="oppo new opcuaapp -n my-app --type Server --url 127.0.0.1 --port 4840"
VAR_COMMANDS[8]="oppo new opcuaapp --name my-app -t Server -u 127.0.0.1 -p 4840"
VAR_COMMANDS[9]="oppo new opcuaapp --name my-app -t Server -u 127.0.0.1 --port 4840"
VAR_COMMANDS[10]="oppo new opcuaapp --name my-app -t Server --url 127.0.0.1 -p 4840"
VAR_COMMANDS[11]="oppo new opcuaapp --name my-app -t Server --url 127.0.0.1 --port 4840"
VAR_COMMANDS[12]="oppo new opcuaapp --name my-app --type Server -u 127.0.0.1 -p 4840"
VAR_COMMANDS[13]="oppo new opcuaapp --name my-app --type Server -u 127.0.0.1 --port 4840"
VAR_COMMANDS[14]="oppo new opcuaapp --name my-app --type Server --url 127.0.0.1 -p 4840"
VAR_COMMANDS[15]="oppo new opcuaapp --name my-app --type Server --url 127.0.0.1 --port 4840"

for INDEX in "${!VAR_COMMANDS[@]}";
do
  VAR_COMMAND=${VAR_COMMANDS[INDEX]}
  
  echo "Testing command '${VAR_COMMAND}' ..."

  mkdir new-opcuaapp-Server--success
  cd    new-opcuaapp-Server--success

  precondition_oppo_log_file_is_not_existent

  ${VAR_COMMAND}

  check_for_exisiting_oppo_log_file

  check_for_exisiting_directory_named "./my-app/models" \
                                      "models directory does not exist ..."

  check_for_exisiting_file_named "./my-app/my-app.oppoproj" \
                                 "oppo project file does not exist ..."

  check_for_exisiting_file_named "./my-app/meson.build" \
                                 "meson.build file does not exist ..."

  check_for_exisiting_file_named "./my-app/src/server/main.c" \
                                 "any oppo project source file for the server application does not exist ..."

  check_for_exisiting_file_named "./my-app/src/server/meson.build" \
                                 "any oppo project source file for the server application does not exist ..."

  check_for_exisiting_file_named "./my-app/src/server/loadInformationModels.c" \
                                 "any oppo project source file for the server application does not exist ..."

  check_for_exisiting_file_named "./my-app/src/server/constants.h" \
                                 "any oppo project source file for the server application does not exist ..."

  check_for_exisiting_file_named "./my-app/src/server/mainCallbacks.c" \
                                 "any oppo project source file for the server application does not exist ..."

  cd ..
  rm -rf new-opcuaapp-Server--success

  echo "Testing command '${VAR_COMMAND}' ... done"
done