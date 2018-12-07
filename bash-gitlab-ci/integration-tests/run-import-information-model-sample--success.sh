#!/bin/bash

set -euo pipefail

source bash-gitlab-ci/util-integration-tests.sh

VAR_COMMANDS[0]="oppo import information-model my-app -s"
VAR_COMMANDS[1]="oppo import information-model my-app --sample"

for INDEX in "${!VAR_COMMANDS[@]}";
do
  VAR_COMMAND=${VAR_COMMANDS[INDEX]}
  
  echo "Testing command '${VAR_COMMAND}' ..."

  mkdir import-information-model--success
  cd    import-information-model--success

  oppo new opcuaapp -n "my-app"
  rm --force "./oppo.log"

  precondition_oppo_log_file_is_not_existent

  ${VAR_COMMAND}

  check_for_exisiting_file_named "./my-app/models/OpcUaDiModel.xml" \
                                 "information-model import failed ..."

  check_for_exisiting_oppo_log_file

  cd ..
  rm -rf import-information-model--success

  echo "Testing command '${VAR_COMMAND}' ... done"
done