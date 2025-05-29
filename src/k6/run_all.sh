#!/bin/sh

TEST_SCRIPTS=" stress.js breakpoint.js spike.js"
API_TEST_TYPES="GetMapById GetWaypointsByMapId Combined"
OTHER_TEST_TYPES="GetImageById PostWaypoint"
SLEEP_DURATION_SECONDS=60
DEFAULT_BASE_URL="http://localhost:8080"

SINGLE_TEST_RUNNER_SCRIPT="./run.sh"

EFFECTIVE_BASE_URL="${1:-$DEFAULT_BASE_URL}"

if [ ! -f "$SINGLE_TEST_RUNNER_SCRIPT" ]; then
    echo "Error: Single test runner script '$SINGLE_TEST_RUNNER_SCRIPT' not found." >&2
    exit 1
elif [ ! -x "$SINGLE_TEST_RUNNER_SCRIPT" ]; then
    echo "Error: Single test runner script '$SINGLE_TEST_RUNNER_SCRIPT' is not executable." >&2
    exit 1
fi

echo "Starting batch k6 test execution..."
echo "Using BASE_URL for k6 tests: $EFFECTIVE_BASE_URL"
echo "Sleep duration between tests: $SLEEP_DURATION_SECONDS seconds."
echo "Usage: $0 [BASE_URL (defaults to $DEFAULT_BASE_URL)]"
echo "" # Added extra newline here

total_tests_run=0
total_tests_failed=0

get_api_stages() {
    case "$1" in
        "stress.js")
            echo "--stage 1m:300 --stage 4m:300 --stage 1m:0"
            ;;
        "spike.js")
            echo "--stage 30s:650 --stage 1m:0"
            ;;
        "breakpoint.js")
            echo "--stage 10m:1000"
            ;;
    esac
}

get_image_stages() {
    case "$1" in
        "stress.js")
            echo "--stage 1m:50 --stage 4m:50 --stage 1m:0"
            ;;
        "spike.js")
            echo "--stage 15s:100 --stage 1m:0"
            ;;
        "breakpoint.js")
            echo "--stage 10m:200"
            ;;
    esac
}

for script_file in $TEST_SCRIPTS; do
    if [ ! -f "$script_file" ]; then
        echo "\nWarning: Test script '$script_file' not found. Skipping." >&2
        continue
    fi

    echo ""
    echo "Processing k6 script: $script_file"
    
    # Loop for API calls (higher targets)
    for test_type in $API_TEST_TYPES; do
        echo "  -> Running $script_file with TEST_TYPE: $test_type (API)"
        total_tests_run=$((total_tests_run + 1))
        
        api_stages=$(get_api_stages "$script_file")

        "$SINGLE_TEST_RUNNER_SCRIPT" "$script_file" \
            --env TEST_TYPE=$test_type \
            --env BASE_URL=$EFFECTIVE_BASE_URL \
            $api_stages
        
        run_exit_code=$?
        if [ $run_exit_code -ne 0 ]; then
            echo "    Completed $script_file (TEST_TYPE: $test_type) with errors (exit code $run_exit_code)." >&2
            total_tests_failed=$((total_tests_failed + 1))
        else
            echo "    Completed $script_file (TEST_TYPE: $test_type) successfully."
        fi

        echo "\nSleeping for $SLEEP_DURATION_SECONDS seconds before potential next test..."
        sleep $SLEEP_DURATION_SECONDS
    done
    
    # Loop for all other test types (lower targets)
    for test_type in $OTHER_TEST_TYPES; do
        echo ""
        echo "  -> Running $script_file with TEST_TYPE: $test_type (Other)"
        total_tests_run=$((total_tests_run + 1))
        
        image_stages=$(get_image_stages "$script_file")

        "$SINGLE_TEST_RUNNER_SCRIPT" "$script_file" \
            --env TEST_TYPE=$test_type \
            --env BASE_URL=$EFFECTIVE_BASE_URL \
            $image_stages
        
        run_exit_code=$?
        if [ $run_exit_code -ne 0 ]; then
            echo "    Completed $script_file (TEST_TYPE: $test_type) with errors (exit code $run_exit_code)." >&2
            total_tests_failed=$((total_tests_failed + 1))
        else
            echo "    Completed $script_file (TEST_TYPE: $test_type) successfully."
        fi

        echo ""
        echo "Sleeping for $SLEEP_DURATION_SECONDS seconds before potential next test..."
        sleep $SLEEP_DURATION_SECONDS
    done
done

echo ""
echo "-------------------------------------"
echo "Batch k6 test execution finished."
echo "Total test configurations run: $total_tests_run"
echo "Total failed: $total_tests_failed"
echo "-------------------------------------"

if [ $total_tests_failed -gt 0 ]; then
    exit 1
else
    exit 0
fi
