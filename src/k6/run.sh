#!/bin/sh

K6_TEST_SCRIPT="$1"

if [ -z "$K6_TEST_SCRIPT" ]; then
  echo "Error: No k6 test script path provided."
  echo "Usage: $0 <k6_test_script.js> [k6_options ...]"
  exit 1
fi

timestamp=$(date +"%Y%m%d_%H%M%S")
test_script_filename=$(basename "$K6_TEST_SCRIPT")
results_dir="results/${test_script_filename%.*}/${timestamp}"

if ! mkdir -p "$results_dir"; then
    echo "Critical Error: Could not create directory '$results_dir'." >&2
    exit 1
fi

base_url_val="http://localhost:8080"
test_type_val="GetImageById"

shift

for arg in "$@"; do
    case "$arg" in
        TEST_TYPE=*)
            test_type_val="${arg#TEST_TYPE=}"
            ;;
        BASE_URL=*)
            base_url_val="${arg#BASE_URL=}"
            ;;
    esac
done

test_info_file="$results_dir/test_info.txt"

echo "K6 Script: $K6_TEST_SCRIPT" >> "$test_info_file"
echo "BASE_URL: $base_url_val" >> "$test_info_file"
echo "TEST_TYPE: $test_type_val" >> "$test_info_file"
echo "" >> "$test_info_file" # Added extra newline here
echo "Test started at: $(date)" >> "$test_info_file"

k6 run "$K6_TEST_SCRIPT" \
  --summary-export="$results_dir/summary.json" \
  --out csv="$results_dir/results.csv" \
  --out json="$results_dir/results.json" \
  "$@"

k6_exit_code=$?

echo "Test ended at: $(date)" >> "$test_info_file"

exit $k6_exit_code
