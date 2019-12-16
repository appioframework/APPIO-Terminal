#!/bin/bash

set -euo pipefail

source bash/util/docker.bash
source bash/util/flags.bash
source bash/util/functions.bash

function run_build_and_test() {
    local TITLE="Running build and test ( $( print_condition_for_build_and_test ) )"

    print_job \
    "${TITLE}"

    if $( should_run_build_and_test ${@} ) ;
    then
        local CI_JOB_ID=$( print_ci_job_id "build-and-test" )

        ci_job_destroy \
        "${CI_JOB_ID}"

        print_job \
        "${TITLE}" \
        "done"
    else
        print_job \
        "${TITLE}" \
        "skipped"
    fi
}

run_build_and_test ${@}
