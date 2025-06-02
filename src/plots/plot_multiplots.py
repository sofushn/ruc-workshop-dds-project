import os
import pandas as pd
import matplotlib.pyplot as plt
from plots import find_results

scenarios = [
    'single_large_results',
    'single_small_results',
    'multi_results',
]
testtypes = ['breakpoint', 'spike', 'stress']
endpoints = ['Combined', 'GetMapById', 'GetWaypointsByMapId', 'PostWaypoint', 'GetImageById']

# Output directory for plots
output_dir = os.path.join('..', '..', '..', 'docs', 'plot-images')
os.makedirs(output_dir, exist_ok=True)

# Build the results dictionary
all_results = {}
for scenario in scenarios:
    base_path = os.path.join(scenario, 'results')
    if os.path.isdir(base_path):
        all_results.update(find_results(base_path))

for testtype in testtypes:
    for endpoint in endpoints:
        fig, axes = plt.subplots(3, 1, figsize=(14, 12), sharex=True)
        found_any = False

        for scenario in scenarios:
            key = f"{scenario}/{testtype}/{endpoint}"
            if key not in all_results:
                continue
            results_csv = all_results[key]
            test_info_txt = os.path.join(os.path.dirname(results_csv), 'test_info.txt')
            test_type_name = None
            with open(test_info_txt, 'r') as f:
                for line in f:
                    if line.startswith('TEST_TYPE:'):
                        test_type_name = line.split(':', 1)[1].strip()
                        break
            csvData = pd.read_csv(results_csv, low_memory=False)
            metric_name = 'http_req_duration'
            duration_data = csvData[csvData['metric_name'] == metric_name]
            if duration_data.empty:
                continue
            min_timestamp = duration_data['timestamp'].min()
            duration_data = duration_data.copy()
            duration_data['timestamp'] = duration_data['timestamp'] - min_timestamp

            # --- Top: Avg duration ---
            duration_grouped = duration_data.groupby('timestamp')['metric_value'].mean().reset_index()
            axes[0].plot(
                duration_grouped['timestamp'],
                duration_grouped['metric_value'],
                label=scenario.replace('_', ' ').title()
            )

            # --- Middle: Number of requests ---
            count_grouped = duration_data.groupby('timestamp').size().reset_index(name='count')
            axes[1].plot(
                count_grouped['timestamp'],
                count_grouped['count'],
                label=scenario.replace('_', ' ').title()
            )

            # --- Bottom: Request fail rate using http_req_failed ---
            if 'http_req_failed' in csvData['metric_name'].values:
                failed_data = csvData[csvData['metric_name'] == 'http_req_failed'].copy()
                if not failed_data.empty:
                    failed_data['timestamp'] = failed_data['timestamp'] - min_timestamp
                    # Fail rate is value * 100 (value is fraction failed)
                    fail_grouped = failed_data.groupby('timestamp')['metric_value'].mean().reset_index()
                    axes[2].plot(
                        fail_grouped['timestamp'],
                        fail_grouped['metric_value'] * 100,
                        label=scenario.replace('_', ' ').title()
                    )
            found_any = True

        if found_any:
            # --- Duration plot ---
            plt.figure(figsize=(14, 4))
            for scenario in scenarios:
                key = f"{scenario}/{testtype}/{endpoint}"
                if key not in all_results:
                    continue
                results_csv = all_results[key]
                csvData = pd.read_csv(results_csv, low_memory=False)
                metric_name = 'http_req_duration'
                duration_data = csvData[csvData['metric_name'] == metric_name]
                if duration_data.empty:
                    continue
                min_timestamp = duration_data['timestamp'].min()
                duration_data = duration_data.copy()
                duration_data['timestamp'] = duration_data['timestamp'] - min_timestamp
                duration_grouped = duration_data.groupby('timestamp')['metric_value'].mean().reset_index()
                plt.plot(
                    duration_grouped['timestamp'],
                    duration_grouped['metric_value'],
                    label=scenario.replace('_', ' ').title()
                )
            plt.ylabel('Avg Duration (ms)')
            plt.yscale('log')
            plt.title('Average HTTP Request Duration (log scale)')
            plt.legend()
            plt.xlabel('Timestamp (Seconds)')
            plt.tight_layout()
            plt.savefig(os.path.join(output_dir, f"{testtype}_{endpoint}_multiplot_lines_duration.svg"))
            plt.close()

            # --- Requests plot ---
            plt.figure(figsize=(14, 4))
            for scenario in scenarios:
                key = f"{scenario}/{testtype}/{endpoint}"
                if key not in all_results:
                    continue
                results_csv = all_results[key]
                csvData = pd.read_csv(results_csv, low_memory=False)
                metric_name = 'http_req_duration'
                duration_data = csvData[csvData['metric_name'] == metric_name]
                if duration_data.empty:
                    continue
                min_timestamp = duration_data['timestamp'].min()
                duration_data = duration_data.copy()
                duration_data['timestamp'] = duration_data['timestamp'] - min_timestamp
                count_grouped = duration_data.groupby('timestamp').size().reset_index(name='count')
                plt.plot(
                    count_grouped['timestamp'],
                    count_grouped['count'],
                    label=scenario.replace('_', ' ').title()
                )
            plt.ylabel('Requests/sec')
            plt.title('Number of Requests per Second')
            plt.legend()
            plt.xlabel('Timestamp (Seconds)')
            plt.tight_layout()
            plt.savefig(os.path.join(output_dir, f"{testtype}_{endpoint}_multiplot_lines_requests.svg"))
            plt.close()

            # --- Check fail rate plot ---
            plt.figure(figsize=(14, 4))
            for scenario in scenarios:
                key = f"{scenario}/{testtype}/{endpoint}"
                if key not in all_results:
                    continue
                results_csv = all_results[key]
                test_info_txt = os.path.join(os.path.dirname(results_csv), 'test_info.txt')
                test_type_name = None
                with open(test_info_txt, 'r') as f:
                    for line in f:
                        if line.startswith('TEST_TYPE:'):
                            test_type_name = line.split(':', 1)[1].strip()
                            break
                csvData = pd.read_csv(results_csv, low_memory=False)
                metric_name = 'http_req_duration'
                duration_data = csvData[csvData['metric_name'] == metric_name]
                if duration_data.empty:
                    continue
                min_timestamp = duration_data['timestamp'].min()
                if 'check' in csvData.columns:
                    check_status = 'status is 201' if test_type_name == 'PostWaypoint' else 'status is 200'
                    check_data = csvData[csvData['check'] == check_status].copy()
                    if not check_data.empty:
                        check_data['timestamp'] = check_data['timestamp'] - min_timestamp
                        check_grouped = check_data.groupby('timestamp')['metric_value'].agg(['mean', 'count']).reset_index()
                        plt.plot(
                            check_grouped['timestamp'],
                            (1 - check_grouped['mean']) * 100,
                            label=scenario.replace('_', ' ').title()
                        )
            plt.ylabel('Check Fail Rate (%)')
            plt.ylim(0, 100)
            plt.title('Check Fail Rate Over Time')
            plt.legend()
            plt.xlabel('Timestamp (Seconds)')
            plt.tight_layout()
            plt.savefig(os.path.join(output_dir, f"{testtype}_{endpoint}_multiplot_lines_check.svg"))
            plt.close()

            # --- Request fail rate plot using http_req_failed ---
            plt.figure(figsize=(14, 4))
            max_fail_rate = 0  # Track the maximum fail rate
            fail_data_per_scenario = []
            for scenario in scenarios:
                key = f"{scenario}/{testtype}/{endpoint}"
                if key not in all_results:
                    continue
                results_csv = all_results[key]
                csvData = pd.read_csv(results_csv, low_memory=False)
                metric_name = 'http_req_failed'
                if metric_name in csvData['metric_name'].values:
                    failed_data = csvData[csvData['metric_name'] == metric_name].copy()
                    if not failed_data.empty:
                        min_timestamp = failed_data['timestamp'].min()
                        failed_data['timestamp'] = failed_data['timestamp'] - min_timestamp
                        fail_grouped = failed_data.groupby('timestamp')['metric_value'].mean().reset_index()
                        fail_grouped['fail_rate_percent'] = fail_grouped['metric_value'] * 100
                        max_fail_rate = max(max_fail_rate, fail_grouped['fail_rate_percent'].max())
                        fail_data_per_scenario.append((fail_grouped, scenario))
            for fail_grouped, scenario in fail_data_per_scenario:
                plt.plot(
                    fail_grouped['timestamp'],
                    fail_grouped['fail_rate_percent'],
                    label=scenario.replace('_', ' ').title()
                )
            plt.ylabel('Request Fail Rate (%)')
            # Avoid singular ylim if max_fail_rate is 0 (no failed requests)
            if max_fail_rate == 0:
                plt.ylim(0, 1)  # Show a minimal range for empty/failure-free plots
            else:
                plt.ylim(0, min(100, max_fail_rate * 1.05))  # Set upper limit to 5% above max, but not above 100%
            plt.title('Request Fail Rate Over Time')
            plt.legend()
            plt.xlabel('Timestamp (Seconds)')
            plt.tight_layout()
            plt.savefig(os.path.join(output_dir, f"{testtype}_{endpoint}_multiplot_lines_failrate.svg"))
            plt.close()