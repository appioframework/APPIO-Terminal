#!/bin/bash

set -uo pipefail

VAR_COMMANDS[0] ="oppo new opcuaapp --name \"my/\-app\""
VAR_COMMANDS[1] ="oppo new opcuaapp -n     \"my/\-app\""
VAR_COMMANDS[2] ="oppo new opcuaapp --name"
VAR_COMMANDS[3] ="oppo new opcuaapp -n"
VAR_COMMANDS[4] ="oppo new opcuaapp --exit"
VAR_COMMANDS[5] ="oppo new opcuaapp -x"
VAR_COMMANDS[6] ="oppo new opcuaapp -n my-app -type Client"
VAR_COMMANDS[7] ="oppo new opcuaapp -n my-app --t Server"
VAR_COMMANDS[8] ="oppo new opcuaapp -p my-app -t ClientServer"
VAR_COMMANDS[9] ="oppo new opcuaapp -n my-app -t"
VAR_COMMANDS[10]="oppo new opcuaapp -n my-app --type"
VAR_COMMANDS[11]="oppo new opcuaapp -n my-app"
VAR_COMMANDS[12]="oppo new opcuaapp --name my-app"

for INDEX in "${!VAR_COMMANDS[@]}";
do
  VAR_COMMAND=${VAR_COMMANDS[INDEX]}
  
  echo "Testing command '${VAR_COMMAND}' ..."

  mkdir new-opcuaapp--failure
  cd    new-opcuaapp--failure

  precondition_oppo_log_file_is_not_existent

  ${VAR_COMMAND}

  check_for_non_zero_error_code
  
  check_for_exisiting_oppo_log_file

  cd ..
  rm -rf new-opcuaapp--failure

  echo "Testing command '${VAR_COMMAND}' ... done"
done