#!/bin/bash

set -uo pipefail

source bash-gitlab-ci/util-integration-tests.sh

VAR_COMMANDS[0]="oppo deploy --name my-app"
VAR_COMMANDS[1]="oppo deploy -n     my-app"

echo "Testing failure of dpkg call ..."

for INDEX in "${!VAR_COMMANDS[@]}";
do
  VAR_COMMAND=${VAR_COMMANDS[INDEX]}
  
  echo "Testing command '${VAR_COMMAND}' ..."

  mkdir build-opcuaapp--failure--extended
  cd    build-opcuaapp--failure--extended

  oppo new opcuaapp --name my-app
  oppo build        --name my-app
  oppo publish      --name my-app
  rm --force "./oppo.log"

  mv "./my-app/publish/server-app" "./my-app/publish/server-app.bak"

  precondition_oppo_log_file_is_not_existent

  ${VAR_COMMAND}

  check_for_non_zero_error_code

  check_for_exisiting_oppo_log_file

  cd     ..
  rm -rf build-opcuaapp--failure--extended

  echo "Testing command '${VAR_COMMAND}' ... done"
done

echo "Testing failure of dpkg call ... done"